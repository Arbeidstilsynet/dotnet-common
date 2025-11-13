using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace Arbeidstilsynet.Common.FeatureFlags.Model;

/// <summary>
/// Settings for Feature Flags using Unleash.
/// </summary>
public record FeatureFlagSettings
{
    /// <summary>
    /// URL to the Unleash server.
    /// </summary>
    [ConfigurationKeyName("Url")]
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// API key for accessing the Unleash server.
    /// </summary>
    [ConfigurationKeyName("ApiKey")]
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Application name.
    /// </summary>
    [ConfigurationKeyName("AppName")]
    public string AppName { get; init; } = string.Empty;

    /// <summary>
    /// Environment name for feature flag context.
    /// </summary>
    [ConfigurationKeyName("Environment")]
    public string Environment { get; init; } = string.Empty;
}
