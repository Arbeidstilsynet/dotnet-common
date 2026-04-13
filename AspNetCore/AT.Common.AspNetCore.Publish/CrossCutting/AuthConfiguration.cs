using System.ComponentModel.DataAnnotations;
using Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;
using Microsoft.Extensions.Configuration;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

/// <summary>
/// Configuration for authentication. This is used to configure the authentication middleware.
///
/// Use <see cref="StartupExtensions.AddStandardAuth"/> to configure your API with it.
/// </summary>
public record AuthConfiguration
{
    /// <summary>
    /// If true, authentication is disabled and all requests are allowed.
    /// </summary>
    /// <remarks>
    /// This should only be used for local development and testing.
    /// </remarks>
    [ConfigurationKeyName("DangerousDisableAuth")]
    public bool DisableAuth { get; init; } = false;

    /// <summary>
    /// The tenant ID for the authentication provider.
    /// </summary>
    [Required]
    [ConfigurationKeyName("TenantId")]
    public required string TenantId { get; init; }

    /// <summary>
    /// The client ID for the authentication provider.
    /// </summary>
    [Required]
    [ConfigurationKeyName("ClientId")]
    public required string ClientId { get; init; }

    /// <summary>
    /// The scope for the authentication provider.
    /// </summary>
    [Required]
    [ConfigurationKeyName("Scope")]
    public required string Scope { get; init; }
}
