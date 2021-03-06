using System;
using System.Collections.Generic;

namespace Machine.Eon.Mapping.Inspection
{
  public class VisitationOptions
  {
    private readonly List<TypeKey> _types;
    private readonly List<MethodKey> _methods;
    private readonly bool _primaryVisitation;

    public bool PrimaryVisitation
    {
      get { return _primaryVisitation; }
    }

    public bool ShouldVisit(TypeKey typeKey)
    {
      if (_types == null)
      {
        return true;
      }
      return _types.Contains(typeKey);
    }

    public bool ShouldVisit(MethodKey methodKey)
    {
      if (_methods == null)
      {
        return true;
      }
      return _methods.Contains(methodKey);
    }

    public VisitationOptions(bool primaryVisitation, List<TypeKey> types, List<MethodKey> methods)
    {
      _primaryVisitation = primaryVisitation;
      _types = types;
      _methods = methods;
    }

    public VisitationOptions(bool visitMethods)
      : this(visitMethods, null, null)
    {
    }
  }
}
