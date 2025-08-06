using ArchUnitNET.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace TestExtensions.ArchUnit.Tests
{
    internal static class Constants
    {
        internal static string NameSpacePrefix = @"Arbeidstilsynet\.Common\.TestExtensions";
        internal static string RootNamespace = $"^({NameSpacePrefix}|{NameSpacePrefix}\\..*)$";
        internal static string ExtensionsNamespace = CreateNamespaceRegex("Extensions");
        internal static string DependencyInjectionNamespace = CreateNamespaceRegex(
            "DependencyInjection"
        );
        internal static string ModelNamespace = CreateNamespaceRegex("Model");

        private static string CreateNamespaceRegex(string namespaceSection)
        {
            return $@"^({NameSpacePrefix}\.{namespaceSection}|{NameSpacePrefix}\.{namespaceSection}\..*|{NameSpacePrefix}\..*\.{namespaceSection}|{NameSpacePrefix}\..*\.{namespaceSection}\..*)$";
        }
    }

    internal static class Layers
    {
        internal static readonly System.Reflection.Assembly TestExtensionsAssembly =
            typeof(Arbeidstilsynet.Common.TestExtensions.IAssemblyInfo).Assembly;
        internal static readonly System.Reflection.Assembly SystemConsoleAssembly =
            typeof(System.Console).Assembly;

        internal static readonly IObjectProvider<IType> TestExtensionsLayer = Types()
            .That()
            .ResideInAssembly(TestExtensionsAssembly)
            .And()
            .DoNotResideInNamespace("Coverlet.Core.Instrumentation.Tracker")
            .As("TestExtensions Layer");

        internal static readonly IObjectProvider<IType> PublicInterfaces = Interfaces()
            .That()
            .Are(TestExtensionsLayer)
            .And()
            .ArePublic()
            .As("public interfaces");

        internal static readonly IObjectProvider<IType> InterfaceImplementations = Classes()
            .That()
            .Are(TestExtensionsLayer)
            .And()
            .AreAssignableTo(PublicInterfaces)
            .And()
            .AreNot(PublicInterfaces)
            .As("interface implementations");

        internal static readonly IObjectProvider<IType> ExportableTypes = Types()
            .That()
            .ResideInNamespaceMatching(Constants.ExtensionsNamespace)
            .Or()
            .ResideInNamespaceMatching(Constants.DependencyInjectionNamespace)
            .Or()
            .ResideInNamespaceMatching(Constants.ModelNamespace)
            .As("inside exportable namespaces");

        internal static readonly IObjectProvider<IType> TypesInInternalNamespaces = Types()
            .That()
            .Are(TestExtensionsLayer)
            .And()
            .AreNot(ExportableTypes)
            .As("outside exportable namespaces");
    }
}
