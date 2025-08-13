using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Arbeidstilsynet.Common.Altinn.Model.Api;

public record MaskinportenTokenResponse
{
    [JsonPropertyName("access_token")]
    [JsonProperty(PropertyName = "access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("token_type")]
    [JsonProperty(PropertyName = "token_type")]
    public string TokenType { get; init; }

    [JsonPropertyName("expires_in")]
    [JsonProperty(PropertyName = "expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("scope")]
    [JsonProperty(PropertyName = "scope")]
    public string Scope { get; init; }
}
