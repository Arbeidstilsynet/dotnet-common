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

    /// <summary>
    /// Optional mapping from role names to Entra ID group object IDs.
    /// </summary>
    /// <remarks>
    /// When set, <see cref="StartupExtensions.AddStandardAuth"/> maps the <c>groups</c> claims of an authenticated user to
    /// <see cref="System.Security.Claims.ClaimTypes.Role"/> claims, so that <c>[Authorize(Roles = "...")]</c> and
    /// <c>User.IsInRole("...")</c> can be used. A user gets a role if they are a member of at least one of its groups.
    ///
    /// Example environment variable: <c>API__Authentication__Roles__MyRole__0=&lt;group-guid&gt;</c>.
    ///
    /// On NAIS, the groups must also be listed under <c>spec.azure.application.claims.groups</c>, otherwise they are not emitted in the token.
    /// </remarks>
    [ConfigurationKeyName("Roles")]
    public IReadOnlyDictionary<string, IReadOnlyList<Guid>> Roles { get; init; } =
        new Dictionary<string, IReadOnlyList<Guid>>();
}
