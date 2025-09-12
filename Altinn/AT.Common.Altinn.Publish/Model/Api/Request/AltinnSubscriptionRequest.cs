using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

public class AltinnSubscriptionRequest
{
    /// <summary>
    /// Endpoint to receive matching events
    /// </summary>
    [JsonPropertyName("endPoint")]
    public Uri? EndPoint { get; set; }

    /// <summary>
    /// Filter on source
    /// </summary>
    [JsonPropertyName("sourceFilter")]
    public Uri? SourceFilter { get; set; }

    /// <summary>
    /// Filter for type. The different sources has different types.
    /// </summary>
    [JsonPropertyName("typeFilter")]
    public string? TypeFilter { get; set; }
}
