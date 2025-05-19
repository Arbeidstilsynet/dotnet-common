using ArchUnitNET.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace FooBarKlient.ArchUnit.Tests
{
    internal static class Constants
    {
        internal static string NameSpacePrefix = $"Arbeidstilsynet\\.Common\\.FooBarKlient";
    }

    internal static class Layers
    {
        internal static readonly System.Reflection.Assembly FooBarKlientPortAssembly =
            typeof(Arbeidstilsynet.Common.FooBarKlient.Ports.IAssemblyInfo).Assembly;

        internal static readonly System.Reflection.Assembly FooBarKlientAdapterAssembly =
            typeof(Arbeidstilsynet.Common.FooBarKlient.Adapters.IAssemblyInfo).Assembly;

        internal static readonly IObjectProvider<IType> FooBarKlientPortLayer = Types()
            .That()
            .ResideInAssembly(FooBarKlientPortAssembly)
            .And()
            .DoNotResideInNamespace("Microsoft.Extensions.DependencyInjection")
            .And()
            .DoNotResideInNamespace("Coverlet.Core.Instrumentation.Tracker")
            .As("FooBarKlient Adapter Layer");

        internal static readonly IObjectProvider<IType> FooBarKlientAdapterLayer = Types()
            .That()
            .ResideInAssembly(FooBarKlientAdapterAssembly)
            .And()
            .DoNotResideInNamespace("Microsoft.Extensions.DependencyInjection")
            .And()
            .DoNotResideInNamespace("Coverlet.Core.Instrumentation.Tracker")
            .As("FooBarKlient Port Layer");
    }
}
