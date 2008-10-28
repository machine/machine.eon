using System;

namespace Machine.Eon.Mapping.Repositories.Impl
{
  public class TypeRepository : ITypeRepository
  {
    private readonly AssemblyRepository _assemblyRepository = new AssemblyRepository();

    public Type FindType(TypeName name)
    {
      Assembly assembly = _assemblyRepository.FindAssembly(name.AssemblyName);
      if (assembly == null)
      {
        assembly = new Assembly(name.AssemblyName);
        _assemblyRepository.SaveAssembly(assembly);
      }
      return assembly.FindOrCreateType(name);
    }
  }
}