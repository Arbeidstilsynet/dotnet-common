namespace Arbeidstilsynet.Common.FeatureFlags.Model;

/// <summary>
/// Request model for feature flag check.
/// </summary>
public record FeatureFlagRequest
{
    /// <summary>
    /// The name of the feature flag to check.
    /// </summary>
    public required string FeatureName { get; init; }

    /// <summary>
    /// Optional context for feature flag evaluation.
    /// </summary>
    public FeatureFlagContext? Context { get; init; }
}
