using System.ComponentModel.DataAnnotations;

namespace Arbeidstilsynet.Common.Saksarkiv.DependencyInjection.Configuration;

/// <summary>
/// Configuration for the Saksarkiv API client.
/// </summary>
public record SaksarkivConfiguration
{
    /// <summary>
    /// Base URL for the Saksarkiv API.
    /// </summary>
    [Url]
    public required string BaseUrl { get; init; }

    /// <summary>
    /// OAuth scope used when requesting access tokens for Saksarkiv.
    /// </summary>
    [Required]
    public required string Scope { get; init; }
}
