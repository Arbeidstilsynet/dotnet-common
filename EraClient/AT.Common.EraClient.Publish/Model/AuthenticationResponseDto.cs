using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.EraClient.Model;

public record AuthenticationResponseDto
{
    /// <summary>
    /// The access token used for authentication.
    /// </summary>
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    /// <summary>
    /// The type of the token, typically "Bearer".
    /// </summary>
    [JsonPropertyName("token_type")]
    public required string TokenType { get; init; }

    /// <summary>
    /// The expiration time of the token in seconds.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public required int ExpiresIn { get; init; }
}
