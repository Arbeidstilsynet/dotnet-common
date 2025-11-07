using Unleash;

namespace Arbeidstilsynet.Common.FeatureFlags.Model;

/// <summary>
/// Represents the context for feature flag evaluation.
/// Only supports UserId at the moment.
/// </summary>
public class FeatureFlagContext
{
    /// <summary>
    /// Gets or sets the user identifier for feature flag evaluation.
    /// </summary>
    public string? UserId { get; set; }
}
