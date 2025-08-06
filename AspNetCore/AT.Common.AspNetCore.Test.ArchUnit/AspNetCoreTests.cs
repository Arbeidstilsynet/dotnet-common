using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
//add a using directive to ArchUnitNET.Fluent.ArchRuleDefinition to easily define ArchRules
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace AspNetCore.ArchUnit.Tests;

public class AspNetCoreAdapterLayerTests
{
    static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(Layers.AspNetCoreAssembly, Layers.SystemConsoleAssembly)
        .Build();

    [Fact]
    public void TypesInAspNetCore_HaveCorrectNamespace()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.AspNetCoreLayer)
            .Should()
            .ResideInNamespaceMatching(Constants.RootNamespace);

        archRule.Check(Architecture);
    }

    [Fact]
    public void InterfaceImplementationsInAspNetCore_AreNotPublic()
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
            .Are(Layers.TypesInInternalNamespaces)
            .Should()
            .NotBePublic()
            .Because(
                "public types should either be an interface OR reside in a namespace containing \"Extensions\", \"DependencyInjection\" or \"Model\"."
            );

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInAspNetCoreAdapterLayer_DoNotDependOnAWS()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.AspNetCoreLayer)
            .Should()
            .NotDependOnAny(Types().That().ResideInNamespaceMatching("^Amazon.*$"));

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInAspNetCoreAdapterLayer_UseCorrectLogger()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.AspNetCoreLayer)
            .Should()
            .NotDependOnAny(typeof(System.Console))
            .Because(
                "we want to use streamlined logging. Try using ILogger<T> via DependencyInjection to log."
            );

        archRule.Check(Architecture);
    }
}
