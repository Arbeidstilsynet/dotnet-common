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

    /// <summary>
    /// Maximum time the Saksarkiv health check waits for the upstream ping before reporting
    /// <see cref="Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded"/>.
    /// </summary>
    /// <remarks>
    /// This must be kept comfortably below the consuming application's readiness probe timeout
    /// (for example the NAIS <c>/healthz/ready</c> probe). Without a short bound, a slow or
    /// unreachable Saksarkiv would block the health endpoint long enough for the readiness probe
    /// to time out, causing the whole pod to be pulled from rotation. Defaults to 800 milliseconds.
    /// </remarks>
    public TimeSpan HealthCheckTimeout { get; init; } = TimeSpan.FromMilliseconds(800);
}
