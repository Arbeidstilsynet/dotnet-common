using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Altinn.Model.Api.Response;

/// <summary>
/// A page of query results.
/// </summary>
/// <typeparam name="T">The type of the items in the page.</typeparam>
public record AltinnQueryResponse<T>
{
    /// <summary>
    /// The number of items in this response.
    /// </summary>
    [JsonPropertyName("count")]
    public long? Count { get; init; }

    /// <summary>
    /// The current query.
    /// </summary>
    [JsonPropertyName("self")]
    public string? Self { get; init; }

    /// <summary>
    /// A link to the next page.
    /// </summary>
    [JsonPropertyName("next")]
    public string? Next { get; init; }

    /// <summary>
    /// The items in this page. Never null; an omitted value yields an empty list.
    /// </summary>
    [JsonPropertyName("instances")]
    public List<T> Instances
    {
        get => _instances;
        init => _instances = value ?? [];
    }

    private readonly List<T> _instances = [];
}
