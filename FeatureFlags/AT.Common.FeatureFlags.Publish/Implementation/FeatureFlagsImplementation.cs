using Arbeidstilsynet.Common.FeatureFlags.Model;
using Arbeidstilsynet.Common.FeatureFlags.Ports;
using Unleash;

namespace Arbeidstilsynet.Common.FeatureFlags.Implementation;

/// <summary>
/// Feature flag implementation using Unleash as the backing service.
/// </summary>
internal class FeatureFlagsImplementation : IFeatureFlags
{
    private readonly IUnleash _unleash;
    private readonly FeatureFlagSettings _featureFlagSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureFlagsImplementation"/> class.
    /// </summary>
    /// <param name="unleash"></param>
    /// <param name="featureFlagSettings"></param>
    public FeatureFlagsImplementation(IUnleash unleash, FeatureFlagSettings featureFlagSettings)
    {
        _unleash = unleash;
        _featureFlagSettings = featureFlagSettings;
    }

    /// <summary>
    /// Checks if a feature flag is enabled.
    /// </summary>
    /// <param name="featureName">Name of feature flag.</param>
    /// <param name="featureFlagContext">Optional context.</param>
    /// <returns></returns>
    public bool IsEnabled(string featureName, FeatureFlagContext? featureFlagContext = null)
    {
        bool isEnabled;
        var unleashContext = new UnleashContext { Environment = _featureFlagSettings.Environment };
        if (featureFlagContext != null && featureFlagContext.UserId != null)
        {
            unleashContext.UserId = featureFlagContext.UserId;
            isEnabled = _unleash.IsEnabled(featureName, unleashContext);
        }
        else
        {
            isEnabled = _unleash.IsEnabled(featureName, unleashContext);
        }

        return isEnabled;
    }
}
