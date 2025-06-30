using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.GeoNorge;

/// <summary>
/// Represents a location on earth with latitude and longitude. Default system is 4258.
/// </summary>
public record Location
{
    [JsonPropertyName("lat")]
    public required double Latitude { get; set; }
    [JsonPropertyName("lon")]
    public required double Longitude { get; set; }
}