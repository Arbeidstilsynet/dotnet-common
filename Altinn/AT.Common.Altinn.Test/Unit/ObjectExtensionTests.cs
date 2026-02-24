using Arbeidstilsynet.Common.Altinn.Extensions;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class ObjectExtensionTests
{
    [Fact]
    public void Merge_ShouldUpdateFields()
    {
        // Arrange
        var original = new TestRecord { Prop1 = "Original", Prop3 = null! };

        var patch = new TestRecord()
        {
            Prop1 = null,
            Prop2 = 100,
            Prop3 = "Updated",
        };

        // Act
        var updated = original.Merge(patch);

        // Assert
        updated.ShouldBeEquivalentTo(original with { Prop2 = patch.Prop2, Prop3 = patch.Prop3 });
    }

    [Fact]
    public void Merge_ShouldNotUpdateFields_WhenPatchHasNullValues()
    {
        // Arrange
        var original = new TestRecord
        {
            Prop1 = "Original1",
            Prop2 = 50,
            Prop3 = "Original2",
        };

        var patch = new TestRecord()
        {
            Prop1 = null,
            Prop2 = 100,
            Prop3 = null!,
        };

        // Act
        var updated = original.Merge(patch);

        // Assert
        updated.ShouldBeEquivalentTo(original with { Prop2 = patch.Prop2 });
    }

    public record TestRecord
    {
        public string? Prop1 { get; init; }
        public int Prop2 { get; init; }
        public required string Prop3 { get; init; }
    }
}
