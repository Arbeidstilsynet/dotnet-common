namespace Arbeidstilsynet.Common.GeoNorge.Model.Request;

public record PointQuery
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}