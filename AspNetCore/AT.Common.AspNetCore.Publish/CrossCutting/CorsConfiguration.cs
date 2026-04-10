using System.ComponentModel.DataAnnotations;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

/// <summary>
/// Configuration for CORS. This is used to configure the CORS middleware.
/// </summary>
public record CorsConfiguration
{
    /// <summary>
    /// A list of allowed origins for CORS requests. This should be a list of allowed domains, e.g. "https://example.com".
    /// If empty or null, CORS will allow any origin in development.
    /// </summary>
    [Required]
    [ConfigurationKeyName("AllowedOrigins")]
    public string[] AllowedOrigins { get; init; } = [];

    /// <summary>
    /// Whether to allow credentials in CORS requests
    /// </summary>
    [Required]
    [ConfigurationKeyName("AllowCredentials")]

    public bool AllowCredentials { get; init; } = false;
}
