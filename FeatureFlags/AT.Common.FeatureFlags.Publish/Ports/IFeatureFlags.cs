using Arbeidstilsynet.Common.FeatureFlags.DependencyInjection;
using Arbeidstilsynet.Common.FeatureFlags.Model;

namespace Arbeidstilsynet.Common.FeatureFlags.Ports;

/// <summary>
/// Use the <see cref="DependencyInjectionExtensions.AddFeatureFlags"/> method to inject an implementation of this interface.
/// </summary>
public interface IFeatureFlags
{
    /// <summary>
    /// Checks if a feature flag is enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to check</param>
    /// <param name="context">Optional context for feature flag evaluation</param>
    /// <returns>True if the feature flag is enabled, false otherwise</returns>
    bool IsEnabled(string featureName, FeatureFlagContext? context = null);
}
