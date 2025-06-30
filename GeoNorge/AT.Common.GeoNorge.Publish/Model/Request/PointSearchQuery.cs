namespace Arbeidstilsynet.Common.GeoNorge;

public record PointSearchQuery
{
    public required Location Point { get; init; }
    public required double RadiusInMeters { get; init; }
}