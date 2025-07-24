using ArchUnitNET.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace MeldingerReceiver.ArchUnit.Tests
{
    internal static class Constants
    {
        internal static string NameSpacePrefix = @"Arbeidstilsynet\.Common\.MeldingerReceiver";
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
        internal static readonly System.Reflection.Assembly MeldingerReceiverAssembly =
            typeof(Arbeidstilsynet.Common.MeldingerReceiver.IAssemblyInfo).Assembly;

        internal static readonly IObjectProvider<IType> MeldingerReceiverLayer = Types()
            .That()
            .ResideInAssembly(MeldingerReceiverAssembly)
            .And()
            .DoNotResideInNamespace("Coverlet.Core.Instrumentation.Tracker")
            .As("MeldingerReceiver Layer");

        internal static readonly IObjectProvider<IType> PublicInterfaces = Interfaces()
            .That()
            .Are(MeldingerReceiverLayer)
            .And()
            .ArePublic()
            .As("public interfaces");

        internal static readonly IObjectProvider<IType> InterfaceImplementations = Classes()
            .That()
            .Are(MeldingerReceiverLayer)
            .And()
            .AreAssignableTo(PublicInterfaces)
            .And()
            .AreNot(PublicInterfaces)
            .As("interface implementations");

        internal static readonly IObjectProvider<IType> PublicAbstractClasses = Classes()
            .That()
            .Are(MeldingerReceiverLayer)
            .And()
            .AreAbstract()
            .And()
            .ArePublic()
            .As("public abstract classes");

        internal static readonly IObjectProvider<IType> ExportableTypes = Types()
            .That()
            .ResideInNamespace(Constants.ExtensionsNamespace, true)
            .Or()
            .ResideInNamespace(Constants.DependencyInjectionNamespace, true)
            .Or()
            .ResideInNamespace(Constants.ModelNamespace, true)
            .As("inside exportable namespaces");

        internal static readonly IObjectProvider<IType> TypesInInternalNamespaces = Types()
            .That()
            .Are(MeldingerReceiverLayer)
            .And()
            .AreNot(ExportableTypes)
            .As("outside exportable namespaces");
    }
}
