namespace Arbeidstilsynet.Common.GeoNorge.Model.Request;

public record PointQuery
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    
    public int Epsg { get; init; } = 4326; // Default to WGS 84 (EPSG:4326)
}