using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
//add a using directive to ArchUnitNET.Fluent.ArchRuleDefinition to easily define ArchRules
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace FooBarKlient.ArchUnit.Tests;

public class FooBarKlientAdapterLayerTests
{
    static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(Layers.FooBarKlientAdapterAssembly)
        .Build();

    [Fact]
    public void TypesInFooBarKlientAdapterLayer_HaveCorrectNamespace()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.FooBarKlientAdapterLayer)
            .Should()
            .ResideInNamespace(
                $"^({Constants.NameSpacePrefix}\\.Adapters|{Constants.NameSpacePrefix}\\.Adapters\\..*)$",
                true
            );

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInFooBarKlientAdapterLayer_AreInternal()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.FooBarKlientAdapterLayer)
            .Should()
            .NotBePublic();

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInFooBarKlientAdapterLayer_DoNotDependOnAWS()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.FooBarKlientAdapterLayer)
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespace("^Amazon.*$", true);

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInFooBarKlientAdapterLayer_UseCorrectLogger()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.FooBarKlientAdapterLayer)
            .Should()
            .NotDependOnAny(typeof(System.Console))
            .Because(
                "We want to use streamlined logging. Try using ILogger<T> via DependencyInjection to log."
            );

        archRule.Check(Architecture);
    }
}
