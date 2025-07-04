namespace Arbeidstilsynet.Common.GeoNorge.Model.Response;

/// <summary>
/// Represents detailed information about a Norwegian municipality (kommune) including its location and county association.
/// </summary>
public record KommuneFullInfo
{
    /// <summary>
    /// 2-digit county number that this municipality belongs to.
    /// </summary>
    public required string Fylkesnummer { get; init; }

    /// <summary>
    /// Basic information about the municipality.
    /// </summary>
    public required Kommune Kommune { get; init; }

    /// <summary>
    /// Geographical location of the municipality center, if available.
    /// </summary>
    public Location? Location { get; init; }
}
