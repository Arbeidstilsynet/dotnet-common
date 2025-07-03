//add a using directive to ArchUnitNET.Fluent.ArchRuleDefinition to easily define ArchRules
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace AT.Common.Altinn.Test.ArchUnit;

public class AltinnAdapterLayerTests
{
    static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(Layers.AltinnAssembly)
        .Build();

    [Fact]
    public void TypesInAltinn_HaveCorrectNamespace()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.AltinnLayer)
            .Should()
            .ResideInNamespace(Constants.RootNamespace, true)
            .WithoutRequiringPositiveResults();

        archRule.Check(Architecture);
    }

    [Fact]
    public void InterfaceImplementationsInAltinn_AreNotPublic()
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
    public void PublicClasses_MustResideInExtensionsOrDependencyInjectionOrModelNamespaces()
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
    public void TypesInAltinnAdapterLayer_DoNotDependOnAWS()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.AltinnLayer)
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespace("^Amazon.*$", true);

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInAltinnAdapterLayer_UseCorrectLogger()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.AltinnLayer)
            .Should()
            .NotDependOnAny(typeof(System.Console))
            .Because(
                "we want to use streamlined logging. Try using ILogger<T> via DependencyInjection to log."
            );

        archRule.Check(Architecture);
    }
}
