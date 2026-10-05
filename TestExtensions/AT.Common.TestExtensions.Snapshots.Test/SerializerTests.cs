using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Test;

/// <summary>Behaviour that is documented in SPEC.md but not covered by the Verify golden corpus.</summary>
public class SerializerTests
{
    [Fact]
    public void JsonElement_IsWrittenAsTree_WithSortedKeys()
    {
        var json = JsonDocument.Parse(
            """{"b": 1.50, "a": "3aa1d561-8c4e-7975-9c9b-f9dff6ede8e5", "c": [true, null, {"z": "x"}], "d": {}}"""
        );

        Snapshot
            .Serialize(new { Doc = json.RootElement })
            .ShouldBe(
                """
                {
                  Doc: {
                    a: Guid_1,
                    b: 1.50,
                    c: [
                      true,
                      null,
                      {
                        z: x
                      }
                    ],
                    d: {}
                  }
                }
                """.ReplaceLineEndings("\n")
            );
    }

    [Fact]
    public void JsonNode_IsWrittenAsTree()
    {
        var node = JsonNode.Parse("""{"name": "n", "count": 2}""");

        Snapshot
            .Serialize(new { Node = node })
            .ShouldBe("{\n  Node: {\n    count: 2,\n    name: n\n  }\n}");
    }

    [Fact]
    public void ThrowingGetter_ThrowsWithMemberAndType()
    {
        var exception = Should.Throw<InvalidOperationException>(() =>
            Snapshot.Serialize(new Golden.Throwing())
        );

        exception.Message.ShouldContain("'Boom'");
        exception.Message.ShouldContain(nameof(Golden.Throwing));
        exception.InnerException!.Message.ShouldBe("boom");
    }

    [Fact]
    public void CultureSpecificDateString_IsNotScrubbed()
    {
        Snapshot.Serialize(new { Date = "28.02.2026" }).ShouldBe("{\n  Date: 28.02.2026\n}");
    }

    [Fact]
    public void SelfReference_IsSkipped()
    {
        var node = new Golden.Node { Name = "root" };
        node.Parent = node;

        Snapshot.Serialize(node).ShouldBe("{\n  Name: root\n}");
    }

    [Fact]
    public void CustomScrubbers_RunInOrder_OnFinalText()
    {
        var settings = new SnapshotSettings()
            .AddScrubber(text => text.Replace("secret", "first"))
            .AddScrubber(text => text.Replace("first", "second"));

        Snapshot.Serialize(new { Value = "secret" }, settings).ShouldBe("{\n  Value: second\n}");
    }

    [Fact]
    public void Settings_CopyConstructor_DoesNotShareScrubbers()
    {
        var original = new SnapshotSettings().AddScrubber(t => t + "!");
        var copy = new SnapshotSettings(original).AddScrubber(t => t + "?");

        Snapshot.Serialize("x", original).ShouldBe("x!");
        Snapshot.Serialize("x", copy).ShouldBe("x!?");
    }

    [Fact]
    public void TimeOnlyMinValue_WithIncludeDefaults_IsScrubbed()
    {
        Snapshot
            .Serialize(
                new { Time = TimeOnly.MinValue },
                new SnapshotSettings().IncludeDefaultValues()
            )
            .ShouldBe("{\n  Time: Time_MinValue\n}");
    }

    [Fact]
    public void StringBuilder_IsWrittenAsText()
    {
        Snapshot.Serialize(new { Text = new StringBuilder("abc") }).ShouldBe("{\n  Text: abc\n}");
    }

    [Fact]
    public void UnboundedGraph_ThrowsInsteadOfOverflowingTheStack()
    {
        var exception = Should.Throw<InvalidOperationException>(() =>
            Snapshot.Serialize(new Endless(0))
        );

        exception.Message.ShouldContain("maximum depth");
    }

    [Fact]
    public void DeepButFiniteGraph_IsSerialized()
    {
        Snapshot.Serialize(new Finite(60)).ShouldContain("Depth: 1");
    }

    private sealed record Endless(int Depth)
    {
        public Endless Next => new(Depth + 1);
    }

    private sealed record Finite(int Depth)
    {
        public Finite? Next => Depth > 1 ? new Finite(Depth - 1) : null;
    }
}
