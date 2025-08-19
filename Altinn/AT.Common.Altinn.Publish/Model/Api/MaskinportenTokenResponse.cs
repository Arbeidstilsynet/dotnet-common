using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Arbeidstilsynet.Common.Altinn.Model.Api;

public record MaskinportenTokenResponse
{
    [JsonPropertyName("access_token")]
    [JsonProperty("access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("token_type")]
    [JsonProperty("token_type")]
    public string TokenType { get; init; }

    [JsonPropertyName("expires_in")]
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("scope")]
    [JsonProperty("scope")]
    public string Scope { get; init; }
}
