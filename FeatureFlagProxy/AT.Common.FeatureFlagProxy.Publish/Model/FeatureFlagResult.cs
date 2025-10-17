namespace Arbeidstilsynet.Common.FeatureFlagProxy.Model;

/// <summary>
/// Represents the result of a feature flag evaluation.
/// </summary>
public record FeatureFlagResult
{
    /// <summary>
    /// The name of the feature flag.
    /// </summary>
    public required string FeatureName { get; init; }

    /// <summary>
    /// Whether the feature flag is enabled.
    /// </summary>
    public required bool IsEnabled { get; init; }

    /// <summary>
    /// The variant name if the feature flag has variants.
    /// </summary>
    public string? Variant { get; init; }

    /// <summary>
    /// The context used for evaluation.
    /// </summary>
    public FeatureFlagContext? Context { get; init; }
}
