namespace Arbeidstilsynet.Common.GeoNorge.Model.Request;

/// <summary>
/// Represents a geographical point query with coordinates and coordinate system specification.
/// </summary>
public record PointQuery
{
    /// <summary>
    /// Latitude in decimal degrees.
    /// </summary>
    public required double Latitude { get; init; }

    /// <summary>
    /// Longitude in decimal degrees.
    /// </summary>
    public required double Longitude { get; init; }
    
    /// <summary>
    /// EPSG code for the coordinate system. Default is 4326 (WGS 84).
    /// </summary>
    public int Epsg { get; init; } = 4326;
}