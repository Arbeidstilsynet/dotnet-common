namespace Arbeidstilsynet.Common.FeatureFlagProxy.Abstract;

/// <summary>
/// Base class for feature flag proxy implementations.
/// Provides common functionality that can be shared across different feature flag providers.
/// </summary>
internal abstract class FeatureFlagProxyBase : IFeatureFlagProxy
{
    /// <summary>
    /// Checks if a feature flag is enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to check.</param>
    /// <returns>True if the feature flag is enabled, false otherwise.</returns>
    public abstract bool IsEnabled(string featureName);

    /// <summary>
    /// Checks if a feature flag is enabled with context.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to check.</param>
    /// <param name="userId">Optional user ID for context.</param>
    /// <param name="properties">Optional additional properties for context.</param>
    /// <returns>True if the feature flag is enabled, false otherwise.</returns>
    public abstract bool IsEnabled(string featureName, string? userId = null, IDictionary<string, string>? properties = null);

    /// <summary>
    /// Validates that a feature name is not null or empty.
    /// </summary>
    /// <param name="featureName">The feature name to validate.</param>
    /// <exception cref="ArgumentException">Thrown when feature name is null or empty.</exception>
    protected static void ValidateFeatureName(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            throw new ArgumentException("Feature name cannot be null or empty.", nameof(featureName));
        }
    }
}
