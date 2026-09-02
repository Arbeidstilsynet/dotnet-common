using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Altinn.Model.Api.Response;

/// <summary>
/// An event subscription registered with Altinn.
/// </summary>
public record AltinnSubscription
{
    /// <summary>
    /// Subscription Id
    /// </summary>
    [JsonPropertyName("id")]
    public int? Id { get; init; }

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
    /// Filter on subject
    /// </summary>
    [JsonPropertyName("subjectFilter")]
    public string? SubjectFilter { get; init; }

    /// <summary>
    /// Filter on alternative subject
    /// </summary>
    [JsonPropertyName("alternativeSubjectFilter")]
    public string? AlternativeSubjectFilter { get; init; }

    /// <summary>
    /// Filter for type. The different sources has different types.
    /// </summary>
    [JsonPropertyName("typeFilter")]
    public string? TypeFilter { get; init; }

    /// <summary>
    /// The events consumer
    /// </summary>
    [JsonPropertyName("consumer")]
    public string? Consumer { get; init; }

    /// <summary>
    /// Who created this subscription
    /// </summary>
    [JsonPropertyName("createdBy")]
    public string? CreatedBy { get; init; }

    /// <summary>
    /// When subscription was created
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime? Created { get; init; }

    /// <summary>
    /// Indicate whether the subscription has been validated to be ok.
    /// </summary>
    [JsonPropertyName("validated")]
    public bool? Validated { get; init; }
}
