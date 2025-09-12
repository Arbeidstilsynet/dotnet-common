using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Altinn.Model.Api.Response;

/// <summary>
/// Query response object
/// </summary>
public class AltinnQueryResponse<T>
{
    /// <summary>
    /// The number of items in this response.
    /// </summary>
    [JsonPropertyName("count")]
    public long Count { get; set; }

    /// <summary>
    /// The current query.
    /// </summary>
    [JsonPropertyName("self")]
    public string Self { get; set; }

    /// <summary>
    /// A link to the next page.
    /// </summary>
    [JsonPropertyName("next")]
    public string Next { get; set; }

    /// <summary>
    /// The metadata.
    /// </summary>
    [JsonPropertyName("instances")]
    public List<T> Instances { get; set; }
}
