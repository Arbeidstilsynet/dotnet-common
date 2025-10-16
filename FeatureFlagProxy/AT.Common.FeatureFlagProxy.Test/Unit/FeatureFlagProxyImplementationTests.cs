using Arbeidstilsynet.Common.FeatureFlagProxy.Implementation;
using Moq;
using Shouldly;
using Unleash;
using Xunit;

namespace Arbeidstilsynet.Common.FeatureFlagProxy.Test;

public class FeatureFlagProxyTests
{
    private readonly FeatureFlagProxyImplementation _sut;
    private readonly Mock<IUnleash> _unleashMock;

    public FeatureFlagProxyTests()
    {
        _unleashMock = new Mock<IUnleash>();
        _sut = new FeatureFlagProxyImplementation(_unleashMock.Object);
    }

    [Fact]
    public void IsEnabled_WhenFeatureIsEnabled_ReturnsTrue()
    {
        // Arrange
        const string featureName = "test-feature";
        _unleashMock.Setup(x => x.IsEnabled(featureName)).Returns(true);

        // Act
        var result = _sut.IsEnabled(featureName);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsEnabled_WhenFeatureIsDisabled_ReturnsFalse()
    {
        // Arrange
        const string featureName = "test-feature";
        _unleashMock.Setup(x => x.IsEnabled(featureName)).Returns(false);

        // Act
        var result = _sut.IsEnabled(featureName);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsEnabled_WithNullFeatureName_ThrowsArgumentException()
    {
        // Act & Assert
        Should
            .Throw<ArgumentException>(() => _sut.IsEnabled(null!))
            .Message.ShouldContain("Feature name cannot be null or empty");
    }

    [Fact]
    public void IsEnabled_WithContext_CallsUnleashWithContext()
    {
        // Arrange
        const string featureName = "test-feature";
        const string userId = "user123";
        var properties = new Dictionary<string, string> { { "region", "norway" } };

        _unleashMock.Setup(x => x.IsEnabled(featureName, It.IsAny<UnleashContext>())).Returns(true);

        // Act
        var result = _sut.IsEnabled(featureName, userId, properties);

        // Assert
        result.ShouldBeTrue();
        _unleashMock.Verify(
            x =>
                x.IsEnabled(
                    featureName,
                    It.Is<UnleashContext>(c =>
                        c.UserId == userId
                        && c.Properties != null
                        && c.Properties.ContainsKey("region")
                        && c.Properties["region"] == "norway"
                    )
                ),
            Times.Once
        );
    }
}
