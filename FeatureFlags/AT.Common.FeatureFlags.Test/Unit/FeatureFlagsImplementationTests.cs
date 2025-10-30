using Arbeidstilsynet.Common.FeatureFlags.Implementation;
using Unleash;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.FeatureFlags.Test.Unit;

public class FeatureFlagsTests
{
    [Fact]
    public void IsEnabled_WhenAllFeaturesIsEnabled_ReturnsTrue()
    {
        var unleash = new FakeUnleash();
        unleash.EnableAllToggles();
        const string featureName = "test-feature";

        var result = unleash.IsEnabled(featureName);
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsEnabled_WhenFeatureIsEnabled_ReturnsTrue()
    {
        var unleash = new FakeUnleash();
        unleash.SetToggle("test-feature", true);
        const string featureName = "test-feature";

        var result = unleash.IsEnabled(featureName);
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsEnabled_WhenAllFeaturesIsDisabled_ReturnsFalse()
    {
        var unleash = new FakeUnleash();
        unleash.DisableAllToggles();
        const string featureName = "test-feature";

        var result = unleash.IsEnabled(featureName);
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsEnabled_WhenFeatureIsDisabled_ReturnsFalse()
    {
        var unleash = new FakeUnleash();
        unleash.SetToggle("test-feature", false);
        const string featureName = "test-feature";

        var result = unleash.IsEnabled(featureName);
        result.ShouldBeFalse();
    }
}
