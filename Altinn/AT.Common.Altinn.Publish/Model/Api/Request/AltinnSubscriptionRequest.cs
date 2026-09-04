using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

/// <summary>
/// Represents a request to create an Altinn event subscription.
/// </summary>
public record AltinnSubscriptionRequest
{
    /// <summary>
    /// Endpoint to receive matching events
    /// </summary>
    [JsonPropertyName("endPoint")]
    public Uri? EndPoint { get; init; }

    /// <summary>
    /// Filter on source
    /// </summary>
    [JsonPropertyName("sourceFilter")]
    public Uri? SourceFilter { get; init; }

    /// <summary>
    /// Filter for type. The different sources has different types.
    /// </summary>
    [JsonPropertyName("typeFilter")]
    public string? TypeFilter { get; init; }
}
