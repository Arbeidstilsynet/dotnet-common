using Arbeidstilsynet.Common.FeatureFlags.Extensions;
using Arbeidstilsynet.Common.FeatureFlags.Model;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.FeatureFlags.Test.Unit;

public class FeatureFlagsExtensionsTests
{
    [Fact]
    public void ToUpper_WhenCalled_ReturnsUppercaseFoo()
    {
        // Arrange
        var model = new FeatureFlagsDto() { Foo = "Bar" };

        // Act
        var result = model.ToUpper();

        // Assert
        result.Foo.ShouldBe("BAR");
    }
}
