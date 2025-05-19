using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
//add a using directive to ArchUnitNET.Fluent.ArchRuleDefinition to easily define ArchRules
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace FooBarKlient.ArchUnit.Tests;

public class FooBarKlientPortLayerTests
{
    static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(Layers.FooBarKlientPortAssembly)
        .Build();

    [Fact]
    public void TypesInFooBarKlientPortLayerLayer_HaveCorrectNamespace()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.FooBarKlientPortLayer)
            .Should()
            .ResideInNamespace(
                $"^({Constants.NameSpacePrefix}\\.Ports|{Constants.NameSpacePrefix}\\.Ports\\..*)$",
                true
            );

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInFooBarKlientPortLayerLayer_ArePublic()
    {
        IArchRule archRule = Types().That().Are(Layers.FooBarKlientPortLayer).Should().BePublic();

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInFooBarKlientPortLayerLayer_DoNotDependOnOtherTypesThanDefaultTypes()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.FooBarKlientPortLayer)
            .Should()
            .NotDependOnAnyTypesThat()
            .DoNotResideInNamespace(
                $"(^Coverlet.Core.Instrumentation.*$|^System.*$|^{Constants.NameSpacePrefix}.*$)",
                true
            );

        archRule.Check(Architecture);
    }

    [Fact]
    public void TypesInFooBarKlientPortLayerLayer_ShouldNotUseLoggerAtAll()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.FooBarKlientPortLayer)
            .Should()
            .NotDependOnAny(typeof(System.Console))
            .Because("This layer describes our core model and should not contain any logic.");

        archRule.Check(Architecture);
    }
}
