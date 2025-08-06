using ArchUnitNET.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace AspNetCore.ArchUnit.Tests
{
    internal static class Constants
    {
        internal static string NameSpacePrefix = @"Arbeidstilsynet\.Common\.AspNetCore";
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
        internal static readonly System.Reflection.Assembly AspNetCoreAssembly =
            typeof(Arbeidstilsynet.Common.AspNetCore.IAssemblyInfo).Assembly;

        internal static readonly System.Reflection.Assembly SystemConsoleAssembly =
            typeof(System.Console).Assembly;

        internal static readonly IObjectProvider<IType> AspNetCoreLayer = Types()
            .That()
            .ResideInAssembly(AspNetCoreAssembly)
            .And()
            .DoNotResideInNamespace("Coverlet.Core.Instrumentation.Tracker")
            .As("AspNetCore Layer");

        internal static readonly IObjectProvider<IType> PublicInterfaces = Interfaces()
            .That()
            .Are(AspNetCoreLayer)
            .And()
            .ArePublic()
            .As("public interfaces");

        internal static readonly IObjectProvider<IType> InterfaceImplementations = Classes()
            .That()
            .Are(AspNetCoreLayer)
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
            .Are(AspNetCoreLayer)
            .And()
            .AreNot(ExportableTypes)
            .As("outside exportable namespaces");
    }
}
