using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
//add a using directive to ArchUnitNET.Fluent.ArchRuleDefinition to easily define ArchRules
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace EraClient.ArchUnit.Tests;

public class EraClientPortLayerTests
{
    static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(Layers.EraClientPortAssembly)
        .Build();

    [Fact]
    public void TypesInEraClientPortLayerLayer_HaveCorrectNamespace()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.EraClientPortLayer)
            .Should()
            .ResideInNamespace(
                $"^({Constants.NameSpacePrefix}\\.Ports|{Constants.NameSpacePrefix}\\.Ports\\..*)$",
                true
            );

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInEraClientPortLayerLayer_ArePublic()
    {
        IArchRule archRule = Types().That().Are(Layers.EraClientPortLayer).Should().BePublic();

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInEraClientPortLayerLayer_DoNotDependOnOtherTypesThanDefaultTypes()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.EraClientPortLayer)
            .Should()
            .NotDependOnAnyTypesThat()
            .DoNotResideInNamespace(
                $"(^Coverlet.Core.Instrumentation.*$|^System.*$|^{Constants.NameSpacePrefix}.*$)",
                true
            );

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInEraClientPortLayerLayer_ShouldNotUseLoggerAtAll()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.EraClientPortLayer)
            .Should()
            .NotDependOnAny(typeof(System.Console))
            .Because("This layer describes our core model and should not contain any logic.");

        archRule.Check(Architecture);
    }
}
