using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

using Assembly = System.Reflection.Assembly;

namespace NuciCraft.API.ArchitectureTests
{
    internal static class ProductionArchitecture
    {
        internal static readonly Assembly[] Assemblies = [typeof(Program).Assembly];

        internal static readonly Architecture Model = new ArchLoader()
            .LoadAssemblies(Assemblies)
            .Build();
    }
}