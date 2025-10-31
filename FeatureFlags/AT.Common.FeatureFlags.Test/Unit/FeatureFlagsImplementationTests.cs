using Arbeidstilsynet.Common.FeatureFlags.Implementation;
using Shouldly;
using Unleash;
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
}
