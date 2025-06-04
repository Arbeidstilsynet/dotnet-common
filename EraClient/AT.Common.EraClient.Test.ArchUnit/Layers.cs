using ArchUnitNET.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace EraClient.ArchUnit.Tests
{
    internal static class Constants
    {
        internal static string NameSpacePrefix = $"Arbeidstilsynet\\.Common\\.EraClient";
    }

    internal static class Layers
    {
        internal static readonly System.Reflection.Assembly EraClientPortAssembly =
            typeof(Arbeidstilsynet.Common.EraClient.Ports.IAssemblyInfo).Assembly;

        internal static readonly System.Reflection.Assembly EraClientAdapterAssembly =
            typeof(Arbeidstilsynet.Common.EraClient.Adapters.IAssemblyInfo).Assembly;

        internal static readonly IObjectProvider<IType> EraClientPortLayer = Types()
            .That()
            .ResideInAssembly(EraClientPortAssembly)
            .And()
            .DoNotResideInNamespace("Coverlet.Core.Instrumentation.Tracker")
            .As("EraClient Port Layer");

        internal static readonly IObjectProvider<IType> EraClientAdapterLayer = Types()
            .That()
            .ResideInAssembly(EraClientAdapterAssembly)
            .And()
            .DoNotResideInNamespace("Coverlet.Core.Instrumentation.Tracker")
            .As("EraClient Adapter Layer");
    }
}
