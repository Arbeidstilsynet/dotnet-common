using Arbeidstilsynet.Common.FeatureFlagProxy.Extensions;
using Arbeidstilsynet.Common.FeatureFlagProxy.Model;
using Moq;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.FeatureFlagProxy.Test;

public class FeatureFlagProxyExtensionsTests
{
    [Fact]
    public void IsEnabled_WithContext_CallsCorrectOverload()
    {
        // Arrange
        var mockProxy = new Mock<IFeatureFlagProxy>();
        const string featureName = "test-feature";
        var context = new FeatureFlagContext
        {
            UserId = "user123",
            Properties = new Dictionary<string, string> { { "region", "norway" } },
        };

        mockProxy
            .Setup(x => x.IsEnabled(featureName, context.UserId, context.Properties))
            .Returns(true);

        // Act
        var result = mockProxy.Object.IsEnabled(featureName, context);

        // Assert
        result.ShouldBeTrue();
        mockProxy.Verify(
            x => x.IsEnabled(featureName, context.UserId, context.Properties),
            Times.Once
        );
    }

    [Fact]
    public void GetResult_WithContext_ReturnsCorrectResult()
    {
        // Arrange
        var mockProxy = new Mock<IFeatureFlagProxy>();
        const string featureName = "test-feature";
        var context = new FeatureFlagContext
        {
            UserId = "user123",
            Properties = new Dictionary<string, string> { { "region", "norway" } },
        };

        mockProxy
            .Setup(x => x.IsEnabled(featureName, context.UserId, context.Properties))
            .Returns(true);

        // Act
        var result = mockProxy.Object.GetResult(featureName, context);

        // Assert
        result.FeatureName.ShouldBe(featureName);
        result.IsEnabled.ShouldBeTrue();
        result.Context.ShouldBe(context);
    }

    [Fact]
    public void GetResult_WithoutContext_ReturnsCorrectResult()
    {
        // Arrange
        var mockProxy = new Mock<IFeatureFlagProxy>();
        const string featureName = "test-feature";

        mockProxy.Setup(x => x.IsEnabled(featureName)).Returns(false);

        // Act
        var result = mockProxy.Object.GetResult(featureName);

        // Assert
        result.FeatureName.ShouldBe(featureName);
        result.IsEnabled.ShouldBeFalse();
        result.Context.ShouldBeNull();
    }
}
