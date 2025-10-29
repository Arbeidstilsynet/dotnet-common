using Arbeidstilsynet.Common.FeatureFlag.Model;
using Unleash;

namespace Arbeidstilsynet.Common.FeatureFlag.Implementation;

/// <summary>
/// Feature flag proxy implementation using Unleash as the backing service.
/// </summary>
internal class FeatureFlagProxyImplementation : IFeatureFlagProxy
{
    private readonly IUnleash _unleash;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureFlagProxyImplementation"/> class.
    /// </summary>
    /// <param name="unleash">The Unleash client instance.</param>
    public FeatureFlagProxyImplementation(IUnleash unleash)
    {
        _unleash = unleash;
    }

    /// <summary>
    /// Checks if a feature flag is enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to check.</param>
    /// <param name="context">Optional context for feature flag evaluation.</param>
    /// <returns>True if the feature flag is enabled, false otherwise.</returns>
    public bool IsEnabled(string featureName, FeatureFlagContext? context = null) =>
        context == null
            ? _unleash.IsEnabled(featureName)
            : _unleash.IsEnabled(featureName, context);
}
