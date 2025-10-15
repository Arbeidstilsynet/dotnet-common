using Arbeidstilsynet.Common.FeatureFlagProxy.Model;

namespace Arbeidstilsynet.Common.FeatureFlagProxy.Extensions;

/// <summary>
/// Extensions for proxy access to feature flags.
/// </summary>
public static class FeatureFlagProxyExtensions
{
    /// <summary>
    /// Extension method to check if a feature flag is enabled with a context object.
    /// </summary>
    /// <param name="featureFlagProxy">The feature flag proxy instance.</param>
    /// <param name="featureName">The name of the feature flag to check.</param>
    /// <param name="context">The context for feature flag evaluation.</param>
    /// <returns>True if the feature flag is enabled, false otherwise.</returns>
    public static bool IsEnabled(this IFeatureFlagProxy featureFlagProxy, string featureName, FeatureFlagContext context)
    {
        return featureFlagProxy.IsEnabled(featureName, context.UserId, context.Properties);
    }

    /// <summary>
    /// Extension method to get a detailed feature flag result.
    /// </summary>
    /// <param name="featureFlagProxy">The feature flag proxy instance.</param>
    /// <param name="featureName">The name of the feature flag to check.</param>
    /// <param name="context">Optional context for feature flag evaluation.</param>
    /// <returns>A detailed result of the feature flag evaluation.</returns>
    public static FeatureFlagResult GetResult(this IFeatureFlagProxy featureFlagProxy, string featureName, FeatureFlagContext? context = null)
    {
        var isEnabled = context != null
            ? featureFlagProxy.IsEnabled(featureName, context.UserId, context.Properties)
            : featureFlagProxy.IsEnabled(featureName);

        return new FeatureFlagResult
        {
            FeatureName = featureName,
            IsEnabled = isEnabled,
            Context = context
        };
    }
}
