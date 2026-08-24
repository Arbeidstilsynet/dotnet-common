using ArchUnitNET.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace AT.Common.Altinn.Test.ArchUnit
{
    internal static class Constants
    {
        internal static string NameSpacePrefix = @"Arbeidstilsynet\.Common\.Altinn";
        internal static string RootNamespace = $"^({NameSpacePrefix}|{NameSpacePrefix}\\..*)$";
        internal static string ExtensionsNamespace = CreateNamespaceRegex("Extensions");
        internal static string DependencyInjectionNamespace = CreateNamespaceRegex(
            "DependencyInjection"
        );
        internal static string ModelNamespace = CreateNamespaceRegex("Model");
        internal static string PortsNamespace = CreateNamespaceRegex("Ports");

        // The Kiota-generated clients are exposed for consumers to adapt locally, in the same way
        // as the GeoNorge package exposes its generated clients.
        internal static string StorageNamespace = CreateNamespaceRegex("Storage");
        internal static string EventsNamespace = CreateNamespaceRegex("Events");
        internal static string AuthenticationNamespace = CreateNamespaceRegex("Authentication");
        internal static string CorrespondenceNamespace = CreateNamespaceRegex("Correspondence");
        internal static string DialogportenNamespace = CreateNamespaceRegex("Dialogporten");
        internal static string AppsNamespace = CreateNamespaceRegex("Apps");

        private static string CreateNamespaceRegex(string namespaceSection)
        {
            return $@"^({NameSpacePrefix}\.{namespaceSection}|{NameSpacePrefix}\.{namespaceSection}\..*|{NameSpacePrefix}\..*\.{namespaceSection}|{NameSpacePrefix}\..*\.{namespaceSection}\..*)$";
        }
    }

    internal static class Layers
    {
        internal static readonly System.Reflection.Assembly AltinnAssembly =
            typeof(Arbeidstilsynet.Common.Altinn.IAssemblyInfo).Assembly;

        internal static readonly System.Reflection.Assembly SystemConsoleAssembly =
            typeof(System.Console).Assembly;
        internal static readonly IObjectProvider<IType> AltinnLayer = Types()
            .That()
            .ResideInAssembly(AltinnAssembly)
            .And()
            .DoNotResideInNamespace("Microsoft.CodeCoverage.Instrumentation.Static.Tracker")
            .As("Altinn Layer");

        internal static readonly IObjectProvider<IType> PublicInterfaces = Interfaces()
            .That()
            .Are(AltinnLayer)
            .And()
            .ArePublic()
            .As("public interfaces");

        internal static readonly IObjectProvider<IType> PublicAbstractClasses = Classes()
            .That()
            .Are(AltinnLayer)
            .And()
            .AreAbstract()
            .And()
            .ArePublic()
            .As("public abstract classes");

        internal static readonly IObjectProvider<IType> InterfaceImplementations = Classes()
            .That()
            .Are(AltinnLayer)
            .And()
            .DoNotResideInNamespaceMatching(Constants.ModelNamespace)
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
            .Or()
            .ResideInNamespaceMatching(Constants.PortsNamespace)
            .Or()
            .ResideInNamespaceMatching(Constants.StorageNamespace)
            .Or()
            .ResideInNamespaceMatching(Constants.EventsNamespace)
            .Or()
            .ResideInNamespaceMatching(Constants.AuthenticationNamespace)
            .Or()
            .ResideInNamespaceMatching(Constants.CorrespondenceNamespace)
            .Or()
            .ResideInNamespaceMatching(Constants.DialogportenNamespace)
            .Or()
            .ResideInNamespaceMatching(Constants.AppsNamespace)
            .As("inside exportable namespaces");

        internal static readonly IObjectProvider<IType> TypesInInternalNamespaces = Types()
            .That()
            .Are(AltinnLayer)
            .And()
            .AreNot(ExportableTypes)
            .As("outside exportable namespaces");
    }
}
