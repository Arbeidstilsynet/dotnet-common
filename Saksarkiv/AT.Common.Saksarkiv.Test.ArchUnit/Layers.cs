using ArchUnitNET.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Saksarkiv.ArchUnit.Tests
{
    internal static class Constants
    {
        internal static string NameSpacePrefix = @"Arbeidstilsynet\.Common\.Saksarkiv";
        internal static string RootNamespace = $"^({NameSpacePrefix}|{NameSpacePrefix}\\..*)$";
        internal static string DependencyInjectionNamespace = CreateNamespaceRegex(
            "DependencyInjection"
        );
        internal static string ModelNamespace = CreateNamespaceRegex("Model");
        internal static string PortsNamespace = CreateNamespaceRegex("Ports");
        internal static string GeneratedNamespace = CreateNamespaceRegex("Generated");

        private static string CreateNamespaceRegex(string namespaceSection)
        {
            return $@"^({NameSpacePrefix}\.{namespaceSection}|{NameSpacePrefix}\.{namespaceSection}\..*|{NameSpacePrefix}\..*\.{namespaceSection}|{NameSpacePrefix}\..*\.{namespaceSection}\..*)$";
        }
    }

    internal static class Layers
    {
        internal static readonly System.Reflection.Assembly SaksarkivAssembly =
            typeof(Arbeidstilsynet.Common.Saksarkiv.IAssemblyInfo).Assembly;

        internal static readonly System.Reflection.Assembly SystemConsoleAssembly =
            typeof(System.Console).Assembly;
        internal static readonly IObjectProvider<IType> SaksarkivLayer = Types()
            .That()
            .ResideInAssembly(SaksarkivAssembly)
            .And()
            .DoNotResideInNamespace("Microsoft.CodeCoverage.Instrumentation.Static.Tracker")
            .As("Saksarkiv Layer");

        internal static readonly IObjectProvider<IType> PublicInterfaces = Interfaces()
            .That()
            .Are(SaksarkivLayer)
            .And()
            .ArePublic()
            .As("public interfaces");

        internal static readonly IObjectProvider<IType> InterfaceImplementations = Classes()
            .That()
            .Are(SaksarkivLayer)
            .And()
            .AreAssignableTo(PublicInterfaces)
            .And()
            .AreNot(PublicInterfaces)
            .As("interface implementations");

        internal static readonly IObjectProvider<IType> ExportableTypes = Types()
            .That()
            .ResideInNamespaceMatching(Constants.DependencyInjectionNamespace)
            .Or()
            .ResideInNamespaceMatching(Constants.ModelNamespace)
            .Or()
            .ResideInNamespaceMatching(Constants.PortsNamespace)
            .Or()
            .ResideInNamespaceMatching(Constants.GeneratedNamespace)
            .As("inside exportable namespaces");

        internal static readonly IObjectProvider<IType> TypesInInternalNamespaces = Types()
            .That()
            .Are(SaksarkivLayer)
            .And()
            .AreNot(ExportableTypes)
            .As("outside exportable namespaces");
    }
}
