using Unleash;

namespace Arbeidstilsynet.Common.FeatureFlags.Model;

/// <summary>
/// Represents the context for feature flag evaluation.
/// </summary>
public class FeatureFlagContext
{
    /// <summary>
    /// Gets or sets the user identifier for feature flag evaluation.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the AppName for feature flag evaluation.
    /// </summary>
    public string? AppName { get; set; }

    /// <summary>
    /// Gets or sets the SessionId for feature flag evaluation.
    /// </summary>
    public string? SessionId { get; set; }
}
