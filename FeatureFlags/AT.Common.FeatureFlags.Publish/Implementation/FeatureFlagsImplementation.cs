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

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureFlagsImplementation"/> class.
    /// </summary>
    /// <param name="unleash"></param>
    public FeatureFlagsImplementation(IUnleash unleash)
    {
        _unleash = unleash;
    }

    /// <summary>
    /// Checks if a feature flag is enabled.
    /// </summary>
    /// <param name="featureName">Name of feature flag.</param>
    /// <param name="context">Optional context.</param>
    /// <returns></returns>
    public bool IsEnabled(string featureName, FeatureFlagContext? context = null) =>
        context == null
            ? _unleash.IsEnabled(featureName)
            : _unleash.IsEnabled(featureName, context);
}
