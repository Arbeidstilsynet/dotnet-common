namespace Arbeidstilsynet.Common.FeatureFlags.Model;

/// <summary>
/// Response model for feature flag check.
/// </summary>
public record FeatureFlagResponse
{
    /// <summary>
    /// Indicates whether the feature flag is enabled.
    /// </summary>
    public required bool IsEnabled { get; init; }

    /// <summary>
    /// The name of the feature flag.
    /// </summary>
    public required string FeatureName { get; init; }
}
