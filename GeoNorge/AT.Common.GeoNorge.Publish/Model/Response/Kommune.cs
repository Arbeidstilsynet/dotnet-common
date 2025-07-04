using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.GeoNorge.Model.Response;

/// <summary>
/// Represents a Norwegian municipality (kommune).
/// </summary>
public record Kommune
{
    /// <summary>
    /// 4-digit municipality number, e.g., "0301" for "Oslo".
    /// </summary>
    [JsonPropertyName("kommunenummer")]
    public required string Kommunenummer { get; init; }

    /// <summary>
    /// Name of the municipality, e.g., "Oslo".
    /// </summary>
    [JsonPropertyName("kommunenavn")]
    public required string Kommunenavn { get; init; }
}