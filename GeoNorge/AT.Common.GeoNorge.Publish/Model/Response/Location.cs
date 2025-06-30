using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.GeoNorge.Model.Response;

/// <summary>
/// Represents a location on earth with latitude and longitude. Default system is 4258.
/// </summary>
public record Location
{
    /// <summary>
    /// Latitude in decimal degrees (WGS84).
    /// </summary>
    [JsonPropertyName("lat")]
    public required double Latitude { get; set; }

    /// <summary>
    /// Longitude in decimal degrees (WGS84).
    /// </summary>
    [JsonPropertyName("lon")]
    public required double Longitude { get; set; }

    /// <summary>
    /// EPSG code for the coordinate system.
    /// </summary>
    [JsonPropertyName("epsg")]
    public string? Epsg { get; set; }
}