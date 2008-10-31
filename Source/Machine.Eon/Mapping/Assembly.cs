using System;
using System.Collections.Generic;

namespace Machine.Eon.Mapping
{
  public class Assembly : Node, IAssembly
  {
    private readonly List<Namespace> _namespaces = new List<Namespace>();
    private readonly AssemblyKey _key;

    public AssemblyKey Key
    {
      get { return _key; }
    }

    public IEnumerable<Namespace> Namespaces
    {
      get { return _namespaces; }
    }

    public Assembly(AssemblyKey key)
    {
      _key = key;
    }

    private Namespace FindOrCreateNamespace(NamespaceKey key)
    {
      foreach (Namespace ns in _namespaces)
      {
        if (ns.Key.Equals(key))
        {
          return ns;
        }
      }
      Namespace newNs = new Namespace(this, key);
      _namespaces.Add(newNs);
      return newNs;
    }

    public Type FindType(TypeKey key)
    {
      Namespace ns = FindOrCreateNamespace(key.Namespace);
      return ns.FindType(key);
    }

    public Type AddType(TypeKey key)
    {
      Namespace ns = FindOrCreateNamespace(key.Namespace);
      return ns.AddType(key);
    }

    public override string ToString()
    {
      return _key.ToString();
    }
  }
}
