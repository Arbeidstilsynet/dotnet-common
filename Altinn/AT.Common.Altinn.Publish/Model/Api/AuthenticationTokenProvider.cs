namespace Arbeidstilsynet.Common.Altinn.Model.Api;

/// <summary>
/// Enum representing the different authentication token providers.
///
/// As of now, only Maskinporten is supported.
/// </summary>
public enum AuthenticationTokenProvider
{
    /// <summary>
    /// Authentication via Altinn App Token
    /// </summary>
    Maskinporten,
}
