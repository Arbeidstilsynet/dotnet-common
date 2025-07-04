using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.GeoNorge.Model.Response;

/// <summary>
/// Represents a Norwegian county (fylke) with its number and name.
/// </summary>
public record Fylke
{
    /// <summary>
    /// 2-digit county number, e.g., "03" for "Oslo".
    /// </summary>
    [JsonPropertyName("fylkesnummer")]
    public required string Fylkesnummer { get; init; }
    /// <summary>
    /// Name of the county, e.g., "Oslo".
    /// </summary>
    [JsonPropertyName("fylkesnavn")]
    public required string Fylkesnavn { get; init; }
}