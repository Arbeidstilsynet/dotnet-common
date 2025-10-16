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
    /// <returns>True if the feature flag is enabled, false otherwise.</returns>
    bool IsEnabled(string featureName);

    /// <summary>
    /// Checks if a feature flag is enabled with context.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to check.</param>
    /// <param name="userId">Optional user ID for context.</param>
    /// <param name="properties">Optional additional properties for context.</param>
    /// <returns>True if the feature flag is enabled, false otherwise.</returns>
    bool IsEnabled(
        string featureName,
        string? userId = null,
        IDictionary<string, string>? properties = null
    );
}
