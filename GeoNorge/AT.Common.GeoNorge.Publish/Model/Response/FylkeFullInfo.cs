namespace Arbeidstilsynet.Common.GeoNorge.Model.Response;

/// <summary>
/// Represents detailed information about a Norwegian county (fylke) including all its municipalities.
/// </summary>
public record FylkeFullInfo
{
    /// <summary>
    /// Basic information about the county.
    /// </summary>
    public required Fylke Fylke { get; init; }
    
    /// <summary>
    /// List of all municipalities within this county.
    /// </summary>
    public required List<KommuneFullInfo> Kommuner { get; init; }
}