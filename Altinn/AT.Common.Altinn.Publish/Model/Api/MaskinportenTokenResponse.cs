using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Altinn.Model.Api;

/// <summary>
/// Represents an access token response from Maskinporten.
/// </summary>
public record MaskinportenTokenResponse
{
    /// <summary>
    /// Gets the access token.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    /// <summary>
    /// Gets the token type.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; }

    /// <summary>
    /// Gets the token lifetime in seconds.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    /// <summary>
    /// Gets the scopes granted to the token.
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; init; }
}
