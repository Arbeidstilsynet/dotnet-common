using Arbeidstilsynet.Common.FeatureFlagProxy.Abstract;
using Unleash;

namespace Arbeidstilsynet.Common.FeatureFlagProxy.Implementation;

/// <summary>
/// Feature flag proxy implementation using Unleash as the backing service.
/// </summary>
internal class FeatureFlagProxyImplementation : FeatureFlagProxyBase
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
    public override bool IsEnabled(string featureName)
    {
        ValidateFeatureName(featureName);
        return _unleash.IsEnabled(featureName);
    }

    /// <summary>
    /// Checks if a feature flag is enabled with context.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to check.</param>
    /// <param name="userId">Optional user ID for context.</param>
    /// <param name="properties">Optional additional properties for context.</param>
    /// <returns>True if the feature flag is enabled, false otherwise.</returns>
    public override bool IsEnabled(
        string featureName,
        string? userId = null,
        IDictionary<string, string>? properties = null
    )
    {
        ValidateFeatureName(featureName);

        if (userId == null && properties == null)
        {
            return _unleash.IsEnabled(featureName);
        }

        var context = new UnleashContext
        {
            UserId = userId,
            Properties = new Dictionary<string, string>(),
        };

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                context.Properties[prop.Key] = prop.Value;
            }
        }

        return _unleash.IsEnabled(featureName, context);
    }
}
