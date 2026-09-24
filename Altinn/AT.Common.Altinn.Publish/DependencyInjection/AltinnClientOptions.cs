namespace Arbeidstilsynet.Common.Altinn.DependencyInjection;

/// <summary>
/// Identifies the Altinn APIs the package can talk to. Each name keys a set of
/// <see cref="AltinnClientOptions"/>, so configuration is per API rather than global.
/// </summary>
public static class AltinnClients
{
    /// <summary>The Altinn storage API.</summary>
    public const string Storage = "Storage";

    /// <summary>The Altinn events API.</summary>
    public const string Events = "Events";

    /// <summary>The Altinn apps API.</summary>
    public const string Apps = "Apps";

    /// <summary>The Altinn correspondence API.</summary>
    public const string Correspondence = "Correspondence";

    /// <summary>The Dialogporten API.</summary>
    public const string Dialogporten = "Dialogporten";
}

/// <summary>
/// Per-API configuration, supplied when registering a client on the Altinn builder.
/// </summary>
/// <remarks>
/// Both properties are optional. Anything left unset falls back to the shared configuration passed
/// to <c>AddAltinn</c>, so a client that needs no special treatment can be added with no options
/// at all.
/// </remarks>
public class AltinnClientOptions
{
    /// <summary>
    /// The base URL for this API, overriding the value derived from the target environment and any
    /// <see cref="AltinnUrlOverrides"/>.
    /// </summary>
    /// <remarks>
    /// Intended for pointing a single client at a mock server. This is subject to the same rule as
    /// <see cref="AltinnUrlOverrides"/>: in Production an override that differs from the resolved
    /// default throws, and in Staging it is logged as a warning.
    /// </remarks>
    public Uri? BaseUrl { get; set; }

    /// <summary>
    /// The Maskinporten scopes to request for this API, overriding
    /// <see cref="MaskinportenConfiguration.Scopes"/>.
    /// </summary>
    /// <remarks>
    /// Altinn scopes are granted per API, so requesting only what a client needs keeps each token
    /// least-privileged. Tokens are cached per distinct scope set, so clients sharing a scope set
    /// share a token.
    /// </remarks>
    public string[]? Scopes { get; set; }
}
