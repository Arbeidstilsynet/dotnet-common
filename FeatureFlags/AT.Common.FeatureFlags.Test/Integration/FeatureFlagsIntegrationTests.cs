using Xunit;
using Arbeidstilsynet.Common.FeatureFlags.Model;
using Arbeidstilsynet.Common.FeatureFlags.Ports;
using Arbeidstilsynet.Common.FeatureFlags.Test.Integration.Setup;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.FeatureFlags.Test.Integration;

public class FeatureFlagsIntegrationTests : TestBed<FeatureFlagsTestFixture>
{
  private readonly IFeatureFlags _sut;

  public FeatureFlagsIntegrationTests(
    ITestOutputHelper testOutputHelper,
    FeatureFlagsTestFixture fixture
  )
    : base(testOutputHelper, fixture)
  {
    _sut = fixture.GetService<IFeatureFlags>(testOutputHelper)!;
  }

  [Fact]
  public void IsEnabled_DefaultSetup_ReturnsFalse()
  {
    // Arrange
    var featureName = "any-feature";

    // Act
    var isEnabled = _sut.IsEnabled(featureName);

    // Assert
    Assert.False(isEnabled);
  }
}