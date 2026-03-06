using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Altinn.Model.Api.Response;

public class AltinnSubscription
{
    /// <summary>
    /// Subscription Id
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

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
    /// Filter on subject
    /// </summary>
    [JsonPropertyName("subjectFilter")]
    public string? SubjectFilter { get; set; }

    /// <summary>
    /// Filter on alternative subject
    /// </summary>
    [JsonPropertyName("alternativeSubjectFilter")]
    public string? AlternativeSubjectFilter { get; set; }

    /// <summary>
    /// Filter for type. The different sources has different types.
    /// </summary>
    [JsonPropertyName("typeFilter")]
    public string? TypeFilter { get; set; }

    /// <summary>
    /// The events consumer
    /// </summary>
    [JsonPropertyName("consumer")]
    public string? Consumer { get; set; }

    /// <summary>
    /// Who created this subscription
    /// </summary>
    [JsonPropertyName("createdBy")]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When subscription was created
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}
