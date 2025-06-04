using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
//add a using directive to ArchUnitNET.Fluent.ArchRuleDefinition to easily define ArchRules
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace EraClient.ArchUnit.Tests;

public class EraClientAdapterLayerTests
{
    static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(Layers.EraClientAdapterAssembly)
        .Build();

    [Fact]
    public void TypesInEraClientAdapterLayer_HaveCorrectNamespace()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.EraClientAdapterLayer)
            .Should()
            .ResideInNamespace(
                $"^({Constants.NameSpacePrefix}\\.Adapters|{Constants.NameSpacePrefix}\\.Adapters\\..*)$",
                true
            );

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInEraClientAdapterLayer_AreInternal()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.EraClientAdapterLayer)
            .And()
            .DoNotResideInNamespace(
                $"^({Constants.NameSpacePrefix}\\.Adapters\\.DependencyInjection|{Constants.NameSpacePrefix}\\.Adapters\\.DependencyInjection\\..*)$",
                true
            )
            .Should()
            .NotBePublic();

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInEraClientAdapterLayer_DoNotDependOnAWS()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.EraClientAdapterLayer)
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespace("^Amazon.*$", true);

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInEraClientAdapterLayer_UseCorrectLogger()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.EraClientAdapterLayer)
            .Should()
            .NotDependOnAny(typeof(System.Console))
            .Because(
                "We want to use streamlined logging. Try using ILogger<T> via DependencyInjection to log."
            );

        archRule.Check(Architecture);
    }
}
