using System;
using System.Collections.Generic;

namespace Machine.Eon.Mapping
{
  public class Namespace : Node, IInNamespace
  {
    private readonly NamespaceName _name;
    private readonly List<Type> _types = new List<Type>();

    public NamespaceName Name
    {
      get { return _name; }
    }

    public NamespaceName NamespaceName
    {
      get { return _name; }
    }

    public IEnumerable<Type> Types
    {
      get { return _types; }
    }

    public Namespace(NamespaceName name)
    {
      _name = name;
    }

    public Type FindOrCreateType(TypeName name)
    {
      if (!name.Namespace.Equals(_name)) throw new ArgumentException("name");
      Type type = FindType(name);
      if (type != null)
      {
        return type;
      }
      type = new Type(name);
      _types.Add(type);
      return type;
    }

    private Type FindType(TypeName name)
    {
      foreach (Type type in _types)
      {
        if (type.Name.Equals(name))
        {
          return type;
        }
      }
      return null;
    }

    public override NodeName NodeName
    {
      get { return _name; }
    }

    public override Usage Usage()
    {
      throw new InvalidOperationException();
    }

    public override string ToString()
    {
      return _name.ToString();
    }
  }
}