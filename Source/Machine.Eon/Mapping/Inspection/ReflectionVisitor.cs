using System;
using System.Collections.Generic;

using Mono.Cecil;

namespace Machine.Eon.Mapping.Inspection
{
  public class ReflectionVisitor : BaseReflectionVisitor
  {
    private readonly Stack<TypeDefinition> _typeDefinitions = new Stack<TypeDefinition>();
    private readonly Stack<TypeReference> _typeReferences = new Stack<TypeReference>();
    private readonly ModelCreator _modelCreator;
    private readonly VisitationOptions _options;

    public ReflectionVisitor(ModelCreator modelCreator, VisitationOptions options)
    {
      _modelCreator = modelCreator;
      _options = options;
    }

    public override void VisitConstructor(MethodDefinition ctor)
    {
      ctor.Body.Accept(new CodeVisitor(_modelCreator));
    }

    public override void VisitConstructorCollection(ConstructorCollection ctors)
    {
      foreach (MethodDefinition ctor in ctors)
      {
        ApplyMethodListeners(ctor);
      }
    }

    public override void VisitCustomAttributeCollection(CustomAttributeCollection customAttrs)
    {
      foreach (CustomAttribute customAttribute in customAttrs)
      {
        _modelCreator.HasAttribute(customAttribute.Constructor.DeclaringType.ToTypeKey());
        customAttribute.Accept(this);
      }
    }

    public override void VisitEventDefinitionCollection(EventDefinitionCollection events)
    {
      foreach (EventDefinition eventDefinition in events)
      {
        _modelCreator.StartEvent(eventDefinition.ToKey());
        eventDefinition.CustomAttributes.Accept(this);
        _modelCreator.SetEventType(eventDefinition.EventType.ToTypeKey());
        _modelCreator.UseType(eventDefinition.EventType.ToTypeKey());
        if (eventDefinition.AddMethod != null)
        {
          ApplyMethodListeners(eventDefinition.AddMethod);
          _modelCreator.SetEventAdder(eventDefinition.ToKey(), eventDefinition.AddMethod.ToMethodKey());
        }
        if (eventDefinition.RemoveMethod != null)
        {
          ApplyMethodListeners(eventDefinition.RemoveMethod);
          _modelCreator.SetEventRemover(eventDefinition.ToKey(), eventDefinition.RemoveMethod.ToMethodKey());
        }
        _modelCreator.EndEvent();
      }
    }

    public override void VisitFieldDefinitionCollection(FieldDefinitionCollection fields)
    {
      foreach (FieldDefinition field in fields)
      {
        _modelCreator.StartField(field.ToFieldKey());
        _modelCreator.SetFieldType(field.FieldType.ToTypeKey());
        _modelCreator.UseType(field.FieldType.ToTypeKey());
        field.Accept(this);
        _modelCreator.EndField();
      }
    }

    public override void VisitInterfaceCollection(InterfaceCollection interfaces)
    {
      foreach (TypeReference type in interfaces)
      {
        _modelCreator.ImplementsInterface(type.ToTypeKey());
        _modelCreator.UseType(type.ToTypeKey());
        type.Accept(this);
      }
    }

    public override void VisitMethodDefinition(MethodDefinition method)
    {
      if (method.Body != null && _options.VisitMethods)
      {
        method.Body.Accept(new CodeVisitor(_modelCreator));
      }
    }

    public override void VisitMethodDefinitionCollection(MethodDefinitionCollection methods)
    {
      foreach (MethodDefinition method in methods)
      {
        ApplyMethodListeners(method);
      }
    }

    public override void VisitNestedTypeCollection(NestedTypeCollection nestedTypes)
    {
      foreach (TypeDefinition type in nestedTypes)
      {
        ApplyTypeVisitors(type);
      }
    }

    public override void VisitOverrideCollection(OverrideCollection overrides)
    {
      foreach (MethodReference method in overrides)
      {
        method.Accept(this);
      }
    }

    public override void VisitParameterDefinitionCollection(ParameterDefinitionCollection parameters)
    {
      foreach (ParameterDefinition parameter in parameters)
      {
        _modelCreator.UseType(parameter.ToTypeKey());
      }
    }

    public override void VisitPropertyDefinition(PropertyDefinition property)
    {
      _modelCreator.StartProperty(property.ToKey());
      _modelCreator.SetPropertyType(property.PropertyType.ToTypeKey());
      _modelCreator.UseType(property.PropertyType.ToTypeKey());
      property.CustomAttributes.Accept(this);
      if (property.GetMethod != null)
      {
        ApplyMethodListeners(property.GetMethod);
        _modelCreator.SetPropertyGetter(property.ToKey(), property.GetMethod.ToMethodKey());
      }
      if (property.SetMethod != null)
      {
        ApplyMethodListeners(property.SetMethod);
        _modelCreator.SetPropertySetter(property.ToKey(), property.SetMethod.ToMethodKey());
      }
      _modelCreator.EndProperty();
    }

    public override void VisitPropertyDefinitionCollection(PropertyDefinitionCollection properties)
    {
      foreach (PropertyDefinition property in properties)
      {
        property.Accept(this);
      }
    }

    public override void VisitTypeDefinitionCollection(TypeDefinitionCollection types)
    {
      foreach (TypeDefinition type in types)
      {
        ApplyTypeVisitors(type);
      }
    }

    private void ApplyTypeVisitors(TypeDefinition type)
    {
      TypeKey typeKey = type.ToTypeKey();
      if (!_options.ShouldVisit(typeKey))
      {
        return;
      }
      if (_typeDefinitions.Contains(type))
      {
        return;
      }
      _typeDefinitions.Push(type);
      _modelCreator.StartNamespace(typeKey.Namespace);
      _modelCreator.StartType(typeKey);
      _modelCreator.SetTypeFlags(type.IsInterface, type.IsAbstract, false);
      if (type.BaseType != null)
      {
        type.BaseType.Accept(this);
        _modelCreator.SetBaseType(type.BaseType.ToTypeKey());
      }
      foreach (GenericParameter parameter in type.GenericParameters)
      {
        foreach (TypeReference reference in parameter.Constraints)
        {
          _modelCreator.UseType(reference.ToTypeKey());
        }
        _modelCreator.UseType(parameter.ToTypeKey());
      }
      type.Accept(this);
      _modelCreator.EndType();
      _modelCreator.EndNamespace();
      _typeDefinitions.Pop();
    }

    public override void VisitTypeReference(TypeReference type)
    {
      if (_typeReferences.Contains(type))
      {
        return;
      }
      _typeReferences.Push(type);
      GenericInstanceType genericInstanceType = type as GenericInstanceType;
      _modelCreator.UseType(type.ToTypeKey());
      if (genericInstanceType != null)
      {
        foreach (TypeReference reference in genericInstanceType.GenericArguments)
        {
          reference.Accept(this);
        }
      }
      _typeReferences.Pop();
    }

    public override void VisitTypeReferenceCollection(TypeReferenceCollection refs)
    {
      foreach (TypeReference reference in refs)
      {
        reference.Accept(this);
      }
    }

    private void ApplyMethodListeners(MethodDefinition method)
    {
      _modelCreator.StartMethod(method.ToKey());
      _modelCreator.SetMethodPrototype(method.ToReturnTypeKey(), method.ToParameterTypeKey());
      _modelCreator.SetMethodFlags(method.IsConstructor, method.IsAbstract, method.IsVirtual, method.IsStatic);
      _modelCreator.UseType(method.ToReturnTypeKey());
      foreach (TypeKey typeName in method.ToParameterTypeKey())
      {
        _modelCreator.UseType(typeName);
      }
      method.Accept(this);
      _modelCreator.EndMethod();
    }
    
    public override void TerminateModuleDefinition(ModuleDefinition module) { }

    public override void VisitCustomAttribute(CustomAttribute customAttr) { }

    public override void VisitEventDefinition(EventDefinition evt) { }

    public override void VisitExternType(TypeReference externType) { }

    public override void VisitExternTypeCollection(ExternTypeCollection externTypes) { }

    public override void VisitFieldDefinition(FieldDefinition field) { }

    public override void VisitGenericParameter(GenericParameter parameter) { }

    public override void VisitGenericParameterCollection(GenericParameterCollection parameters) { }

    public override void VisitInterface(TypeReference interf) { }

    public override void VisitMarshalSpec(MarshalSpec marshalSpec) { }

    public override void VisitMemberReference(MemberReference member) { }

    public override void VisitMemberReferenceCollection(MemberReferenceCollection members) { }

    public override void VisitModuleDefinition(ModuleDefinition module) { }

    public override void VisitNestedType(TypeDefinition nestedType) { }

    public override void VisitOverride(MethodReference ov) { }

    public override void VisitPInvokeInfo(PInvokeInfo pinvk) { }

    public override void VisitSecurityDeclaration(SecurityDeclaration secDecl) { }

    public override void VisitSecurityDeclarationCollection(SecurityDeclarationCollection securityDeclarations) { }

    public override void VisitTypeDefinition(TypeDefinition type) { }

    public override void VisitParameterDefinition(ParameterDefinition parameter) { }
  }
}