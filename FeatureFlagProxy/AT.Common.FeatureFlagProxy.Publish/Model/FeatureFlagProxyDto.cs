namespace Arbeidstilsynet.Common.FeatureFlagProxy.Model;

/// <summary>
/// Represents the context for feature flag evaluation.
/// Mirrors Unleash's UnleashContext for strongly-typed context properties.
/// </summary>
public record FeatureFlagContext
{
    /// <summary>
    /// User ID for context-based feature flag evaluation.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Session ID for the current user session.
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Remote IP address of the user.
    /// </summary>
    public string? RemoteAddress { get; init; }

    /// <summary>
    /// Environment where the application is running (e.g., "development", "staging", "production").
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Application name.
    /// </summary>
    public string? AppName { get; init; }

    /// <summary>
    /// Additional custom properties for feature flag evaluation.
    /// Use this for properties not covered by the strongly-typed properties above.
    /// </summary>
    public IDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();
}

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
