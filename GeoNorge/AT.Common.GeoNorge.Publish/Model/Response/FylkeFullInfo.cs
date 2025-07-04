namespace Arbeidstilsynet.Common.GeoNorge.Model.Response;

public record FylkeFullInfo
{
    public required Fylke Fylke { get; init; }
    
    public required List<KommuneFullInfo> Kommuner { get; init; }
}