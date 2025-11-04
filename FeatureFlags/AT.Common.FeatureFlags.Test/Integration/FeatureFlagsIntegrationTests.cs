using Arbeidstilsynet.Common.FeatureFlags.Model;
using Arbeidstilsynet.Common.FeatureFlags.Ports;
using Arbeidstilsynet.Common.FeatureFlags.Test.Integration.Setup;
using Unleash;
using Xunit;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.FeatureFlags.Test.Integration;

public class FeatureFlagsIntegrationTests : TestBed<FeatureFlagsTestFixture>
{
    private readonly IFeatureFlags _sut;
    private readonly FakeUnleash _fakeUnleash;

    public FeatureFlagsIntegrationTests(
        ITestOutputHelper testOutputHelper,
        FeatureFlagsTestFixture fixture
    )
        : base(testOutputHelper, fixture)
    {
        _sut = fixture.GetService<IFeatureFlags>(testOutputHelper)!;
        _fakeUnleash = (FakeUnleash)fixture.GetService<IUnleash>(testOutputHelper)!;
    }

    [Fact]
    public void IsEnabled_DefaultSetup_ReturnsFalse()
    {
        // Arrange
        var featureName = "any-feature";
        _fakeUnleash.SetToggle(featureName, false);

        // Act
        var isEnabled = _sut.IsEnabled(featureName);

        // Assert
        Assert.False(isEnabled);
    }

    [Fact]
    public void IsEnabled_DefaultSetup_ReturnsTrue()
    {
        // Arrange
        var featureName = "any-feature";
        _fakeUnleash.SetToggle(featureName, true);

        // Act
        var isEnabled = _sut.IsEnabled(featureName);

        // Assert
        Assert.True(isEnabled);
    }
}
