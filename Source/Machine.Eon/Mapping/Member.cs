using System;
using System.Collections.Generic;

namespace Machine.Eon.Mapping
{
  public abstract class Member : Node, IMember, ICanHaveAttributes
  {
    private readonly Type _type;
    private readonly MemberName _name;
    private readonly List<Type> _attributes = new List<Type>();

    public Type Type
    {
      get { return _type; }
    }

    public IEnumerable<Type> Attributes
    {
      get { return _attributes; }
    }

    protected Member(Type type, MemberName name)
    {
      _type = type;
      _name = name;
    }

    public void AddAttribute(Type type)
    {
      if (!_attributes.Contains(type))
      {
        _attributes.Add(type);
      }
    }

    public override string ToString()
    {
      return _name.ToString();
    }
  }
}
