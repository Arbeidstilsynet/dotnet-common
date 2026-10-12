using System.Net.Mime;
using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

/// <summary>
/// An Altinn event, as delivered to a subscription's webhook endpoint.
/// </summary>
/// <remarks>
/// <para>
/// This is deliberately hand-written and must not be replaced by the generated
/// <c>Events.Models.CloudEvent</c>, despite the two looking equivalent.
/// </para>
/// <para>
/// The events specification models <c>specversion</c> as an object rather than a string: it
/// describes the CloudEvents specification's own attribute metadata, which is the
/// <c>CloudNative.CloudEvents</c> library's <c>CloudEventsSpecVersion</c> class leaking into the
/// specification through reflection-based schema generation. Altinn actually sends
/// <c>"specversion": "1.0"</c>, so the generated model cannot deserialize a real event at all --
/// binding one throws <see cref="System.Text.Json.JsonException"/>, which an
/// <c>[ApiController]</c> turns into a 400 before the action body runs.
/// </para>
/// <para>
/// That would break subscription setup rather than merely dropping events: Altinn sends a
/// <c>platform.events.validatesubscription</c> event and requires a success response before it
/// activates a subscription, so a webhook bound to the generated model would never receive
/// anything.
/// </para>
/// <para>
/// This type is also the more useful shape for a consumer: it carries
/// <see cref="System.Text.Json.Serialization.JsonPropertyNameAttribute"/> annotations matching the
/// wire format, exposes <see cref="Source"/> as a <see cref="Uri"/> (which the instance addressing
/// relies on) rather than a string, and includes <see cref="AlternativeSubject"/>, which the
/// generated model omits.
/// </para>
/// </remarks>
public record AltinnCloudEvent
{
    /// <summary>
    /// Gets or sets the id of the event.
    /// </summary>
    [JsonPropertyName("id")]
#nullable disable
    public string Id { get; init; }

    /// <summary>
    /// Gets or sets the source of the event.
    /// </summary>
    [JsonPropertyName("source")]
    public Uri Source { get; init; }

    /// <summary>
    /// Gets or sets the specification version of the event.
    /// </summary>
    [JsonPropertyName("specversion")]
    public string SpecVersion { get; init; }

    /// <summary>
    /// Gets or sets the type of the event.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; }

    /// <summary>
    /// Gets or sets the subject of the event.
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; init; }

#nullable restore

    /// <summary>
    /// Gets or sets the time of the event.
    /// </summary>
    [JsonPropertyName("time")]
    public DateTime Time { get; init; }

    /// <summary>
    /// Gets or sets the alternative subject of the event.
    /// </summary>
    [JsonPropertyName("alternativesubject")]
#nullable disable
    public string AlternativeSubject { get; init; }

    /// <summary>
    /// Gets or sets the cloudEvent data content. The event payload.
    /// The payload depends on the type and the dataschema.
    /// </summary>
    [JsonPropertyName("data")]
    public object Data { get; init; }

    /// <summary>
    /// Gets or sets the cloudEvent dataschema attribute.
    /// A link to the schema that the data attribute adheres to.
    /// </summary>
    [JsonPropertyName("dataschema")]
    public Uri DataSchema { get; init; }

    /// <summary>
    /// Gets or sets the cloudEvent datacontenttype attribute.
    /// Content type of the data attribute value.
    /// </summary>
    [JsonPropertyName("contenttype")]
    public ContentType DataContentType { get; init; }
#nullable restore
}
