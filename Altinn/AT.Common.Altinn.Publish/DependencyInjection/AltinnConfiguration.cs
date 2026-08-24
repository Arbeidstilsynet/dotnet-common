using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Arbeidstilsynet.Common.Altinn.DependencyInjection;

/// <summary>
/// The Altinn instance to communicate with.
/// </summary>
/// <remarks>
/// This is deliberately distinct from the ASP.NET Core host environment: several host environments
/// (Development, Test, QA, ...) can legitimately target either TT02 or a local Altinn instance, so
/// the target is stated explicitly rather than guessed.
/// </remarks>
public enum AltinnEnvironment
{
    /// <summary>
    /// The production Altinn instance (platform.altinn.no).
    /// </summary>
    Production,

    /// <summary>
    /// The TT02 test instance (platform.tt02.altinn.no).
    /// </summary>
    Tt02,

    /// <summary>
    /// A locally running Altinn instance (local.altinn.cloud:8000).
    /// </summary>
    Local,
}

/// <summary>
/// Configuration for Altinn 3 APIs.
/// </summary>
public record AltinnConfiguration
{
    /// <summary>
    /// The organization ID in Altinn. Default is "dat" (Arbeidstilsynet).
    /// </summary>
    public string OrgId { get; init; } = "dat";

    /// <summary>
    /// The Altinn instance to target.
    /// <br/>
    /// Resolution rules, based on the <see cref="IWebHostEnvironment"/> passed to the registration method:
    /// <list type="bullet">
    /// <item><description>Production: always <see cref="AltinnEnvironment.Production"/>. Setting a different value throws.</description></item>
    /// <item><description>Staging: defaults to <see cref="AltinnEnvironment.Tt02"/>.</description></item>
    /// <item><description>Any other environment (Development, Test, QA, ...): required. Registration throws if not set.</description></item>
    /// </list>
    /// </summary>
    public AltinnEnvironment? Environment { get; init; }

    /// <summary>
    /// Overrides for the base URLs that would otherwise be derived from <see cref="Environment"/>.
    /// <br/>
    /// Intended for testing against a mock server. In Production any override that differs from the
    /// resolved default throws; in Staging overrides are permitted but logged as a warning.
    /// </summary>
    public AltinnUrlOverrides? Overrides { get; init; }
}

/// <summary>
/// Base URL overrides for the Altinn APIs. Every property is optional; unset properties are derived
/// from <see cref="AltinnConfiguration.Environment"/>.
/// </summary>
/// <remarks>
/// Each value is the base URL the underlying API client is given, so the API-specific base path is
/// appended automatically. Setting <see cref="PlatformUrl"/> to <c>http://localhost:1234/</c> resolves
/// the storage client to <c>http://localhost:1234/storage/api/v1</c>, and so on. The per-API properties
/// bypass that derivation and are used verbatim.
/// </remarks>
public record AltinnUrlOverrides
{
    /// <summary>
    /// Overrides the Altinn platform base URL, from which the authentication, storage, events,
    /// correspondence and Dialogporten base URLs are derived.
    /// </summary>
    public Uri? PlatformUrl { get; init; }

    /// <summary>
    /// Overrides the base URL for Altinn applications.
    /// </summary>
    public Uri? AppBaseUrl { get; init; }

    /// <summary>
    /// Overrides the base URL for Maskinporten.
    /// </summary>
    public Uri? MaskinportenUrl { get; init; }

    /// <summary>
    /// Overrides the base URL for the Altinn authentication API. See https://docs.altinn.studio/nb/api/authentication/spec/
    /// </summary>
    public Uri? AuthenticationUrl { get; init; }

    /// <summary>
    /// Overrides the base URL for the Altinn storage API. See https://docs.altinn.studio/nb/api/storage/spec/
    /// </summary>
    public Uri? StorageUrl { get; init; }

    /// <summary>
    /// Overrides the base URL for the Altinn events API. See https://docs.altinn.studio/events/api/openapi/
    /// </summary>
    public Uri? EventsUrl { get; init; }

    /// <summary>
    /// Overrides the base URL for the Altinn correspondence API. See https://docs.altinn.studio/nb/api/correspondence/spec/
    /// </summary>
    public Uri? CorrespondenceUrl { get; init; }

    /// <summary>
    /// Overrides the base URL for the Dialogporten API. See https://docs.altinn.studio/en/dialogporten
    /// </summary>
    public Uri? DialogportenUrl { get; init; }
}

/// <summary>
/// Configuration for Altinn authentication.
/// </summary>
public record MaskinportenConfiguration
{
    /// <summary>
    /// The private (rsa) key base64 encoded for the certificate used for authentication.
    /// </summary>
    [Required]
    [ConfigurationKeyName("CertificatePrivateKey")]
    public required string PrivateKey { get; init; }

    /// <summary>
    /// The certificate chain base64 encoded to be used as x5c header.
    /// Required if we do not have uploaded a public key on the maskinporten integration.
    /// </summary>
    [ConfigurationKeyName("CertificateChain")]
    public string? CertificateChain { get; init; }

    /// <summary>
    /// The Key ID (kid) to include in the JWT header.
    /// Required if a public key has been pre-registered in Maskinporten.
    /// </summary>
    [ConfigurationKeyName("KeyId")]
    public string? KeyId { get; init; }

    /// <summary>
    /// The integration ID for the Altinn application.
    /// </summary>
    [Required]
    [ConfigurationKeyName("IntegrationId")]
    public required string IntegrationId { get; init; }

    /// <summary>
    /// The scopes to request during authentication.
    /// </summary>
    [Required]
    [ConfigurationKeyName("Scopes")]
    public required string[] Scopes { get; init; }
}
