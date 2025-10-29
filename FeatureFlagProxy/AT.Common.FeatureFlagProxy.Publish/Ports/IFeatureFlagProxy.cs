using Arbeidstilsynet.Common.FeatureFlag.Model;

namespace Arbeidstilsynet.Common.FeatureFlag;

/// <summary>
/// Interface for feature flag operations
/// </summary>
public interface IFeatureFlag
{
    /// <summary>
    /// Checks if a feature flag is enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to check.</param>
    /// <param name="context">Optional context for feature flag evaluation.</param>
    /// <returns>True if the feature flag is enabled, false otherwise.</returns>
    bool IsEnabled(string featureName, FeatureFlagContext? context = null);
}
