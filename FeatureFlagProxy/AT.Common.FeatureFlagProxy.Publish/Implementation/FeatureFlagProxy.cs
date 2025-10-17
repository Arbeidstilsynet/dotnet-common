using Arbeidstilsynet.Common.FeatureFlagProxy.Model;
using Unleash;

namespace Arbeidstilsynet.Common.FeatureFlagProxy.Implementation;

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
        _unleash = unleash ?? throw new ArgumentNullException(nameof(unleash));
    }

    /// <summary>
    /// Checks if a feature flag is enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to check.</param>
    /// <returns>True if the feature flag is enabled, false otherwise.</returns>
    public bool IsEnabled(string featureName)
    {
        ValidateFeatureName(featureName);
        return _unleash.IsEnabled(featureName);
    }

    /// <summary>
    /// Checks if a feature flag is enabled with context.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to check.</param>
    /// <param name="context">Optional context for feature flag evaluation.</param>
    /// <returns>True if the feature flag is enabled, false otherwise.</returns>
    public bool IsEnabled(string featureName, FeatureFlagContext? context = null)
    {
        ValidateFeatureName(featureName);

        if (context == null)
        {
            return _unleash.IsEnabled(featureName);
        }

        var unleashContext = new UnleashContext
        {
            UserId = context.UserId,
            SessionId = context.SessionId,
            RemoteAddress = context.RemoteAddress,
            Environment = context.Environment,
            AppName = context.AppName,
            Properties = new Dictionary<string, string>(),
        };

        // Add any custom properties
        if (context.Properties != null)
        {
            foreach (var prop in context.Properties)
            {
                unleashContext.Properties[prop.Key] = prop.Value;
            }
        }

        return _unleash.IsEnabled(featureName, unleashContext);
    }

    /// <summary>
    /// Validates that a feature name is not null or empty.
    /// </summary>
    /// <param name="featureName">The feature name to validate.</param>
    /// <exception cref="ArgumentException">Thrown when feature name is null or empty.</exception>
    private static void ValidateFeatureName(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            throw new ArgumentException(
                "Feature name cannot be null or empty.",
                nameof(featureName)
            );
        }
    }
}
