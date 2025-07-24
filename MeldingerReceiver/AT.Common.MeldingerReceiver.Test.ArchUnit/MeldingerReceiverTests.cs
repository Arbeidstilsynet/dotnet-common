using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
//add a using directive to ArchUnitNET.Fluent.ArchRuleDefinition to easily define ArchRules
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace MeldingerReceiver.ArchUnit.Tests;

public class MeldingerReceiverAdapterLayerTests
{
    static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(Layers.MeldingerReceiverAssembly)
        .Build();

    [Fact]
    public void TypesInMeldingerReceiver_HaveCorrectNamespace()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.MeldingerReceiverLayer)
            .Should()
            .ResideInNamespace(Constants.RootNamespace, true)
            .WithoutRequiringPositiveResults();

        archRule.Check(Architecture);
    }

    [Fact]
    public void InterfaceImplementationsInMeldingerReceiver_AreNotPublic()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.InterfaceImplementations)
            .Should()
            .NotBePublic()
            .WithoutRequiringPositiveResults();

        archRule.Check(Architecture);
    }

    [Fact]
    public void PublicNonAbstractClasses_MustResideInExtensionsOrDependencyInjectionOrModelNamespaces()
    {
        IArchRule archRule = Types()
            .That()
            .AreNot(Layers.PublicInterfaces)
            .And()
            .AreNot(Layers.PublicAbstractClasses)
            .And()
            .Are(Layers.TypesInInternalNamespaces)
            .Should()
            .NotBePublic()
            .Because(
                "public types should either be an abstract class, an interface OR reside in a namespace containing \"Extensions\", \"DependencyInjection\" or \"Model\"."
            )
            .WithoutRequiringPositiveResults();

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInMeldingerReceiverAdapterLayer_DoNotDependOnAWS()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.MeldingerReceiverLayer)
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespace("^Amazon.*$", true);

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInMeldingerReceiverAdapterLayer_UseCorrectLogger()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.MeldingerReceiverLayer)
            .Should()
            .NotDependOnAny(typeof(System.Console))
            .Because(
                "we want to use streamlined logging. Try using ILogger<T> via DependencyInjection to log."
            );

        archRule.Check(Architecture);
    }
}
