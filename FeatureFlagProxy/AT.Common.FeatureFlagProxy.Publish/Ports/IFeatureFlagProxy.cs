using Unleash;

namespace Arbeidstilsynet.Common.FeatureFlagProxy;

/// <summary>
/// Interface for feature flag operations
/// </summary>
public interface IFeatureFlagProxy
{
    /// <summary>
    /// Checks if a feature flag is enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to check.</param>
    /// <param name="context">Optional Unleash context for feature flag evaluation.</param>
    /// <returns>True if the feature flag is enabled, false otherwise.</returns>
    bool IsEnabled(string featureName, UnleashContext? context = null);
}
