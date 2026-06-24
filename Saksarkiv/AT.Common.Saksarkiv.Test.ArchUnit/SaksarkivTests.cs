using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Saksarkiv.ArchUnit.Tests;

public class SaksarkivLayerTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(Layers.SaksarkivAssembly, Layers.SystemConsoleAssembly)
        .Build();

    [Fact]
    public void TypesInSaksarkiv_HaveCorrectNamespace()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.SaksarkivLayer)
            .Should()
            .ResideInNamespaceMatching(Constants.RootNamespace)
            .WithoutRequiringPositiveResults();

        archRule.Check(Architecture);
    }

    [Fact]
    public void InterfaceImplementationsInSaksarkiv_AreNotPublic()
    {
        IArchRule archRule = Types()
            .That()
            .Are(Layers.InterfaceImplementations)
            .Should()
            .NotBePublic()
            .WithoutRequiringPositiveResults();

        archRule.Check(Architecture);
    }

}
