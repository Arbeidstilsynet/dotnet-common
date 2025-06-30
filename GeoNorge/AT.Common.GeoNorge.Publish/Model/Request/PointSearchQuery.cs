namespace Arbeidstilsynet.Common.GeoNorge.Model.Request;

/// <summary>
/// Represents a query for searching locations based on a point defined by latitude and longitude.
/// </summary>
public record PointSearchQuery
{
    /// <summary>
    /// Represents a query for searching locations based on a point defined by latitude and longitude.
    /// </summary>
    public required double Latitude { get; init; }
    
    /// <summary>
    /// Longitude in decimal degrees (WGS84).
    /// </summary>
    public required double Longitude { get; init; }
    
    /// <summary>
    /// Radius in meters around the point to search for locations.
    /// </summary>
    public required double RadiusInMeters { get; init; }
}