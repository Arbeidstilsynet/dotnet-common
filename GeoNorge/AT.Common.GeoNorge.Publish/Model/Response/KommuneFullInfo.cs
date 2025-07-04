namespace Arbeidstilsynet.Common.GeoNorge.Model.Response;

public record KommuneFullInfo
{
    public required string Fylkesnummer { get; init; }
    public required Kommune Kommune { get; init; }
    public Location? Location { get; init; }
}