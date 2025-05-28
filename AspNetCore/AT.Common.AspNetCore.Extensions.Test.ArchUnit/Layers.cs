using ArchUnitNET.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Extensions.ArchUnit.Tests
{
    internal static class Constants
    {
        internal static string NameSpacePrefix = $"Arbeidstilsynet\\.Common\\.AspNetCore";
    }

    internal static class Layers
    {
        internal static readonly System.Reflection.Assembly ExtensionsAssembly =
            typeof(Arbeidstilsynet.Common.AspNetCore.Extensions.IAssemblyInfo).Assembly;

        internal static readonly IObjectProvider<IType> ExtensionsLayer = Types()
            .That()
            .ResideInAssembly(ExtensionsAssembly)
            .And()
            .DoNotResideInNamespace("Coverlet.Core.Instrumentation.Tracker")
            .As("Extensions Port Layer");
    }
}
