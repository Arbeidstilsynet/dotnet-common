namespace Arbeidstilsynet.Common.GeoNorge.Model.Request;

public record TextSearchQuery
{
    public required string SearchTerm { get; init; }
    public bool FuzzySearch { get; init; } = false;
    
    public string? Adressenavn { get; init; }
    public string? Poststed { get; init; }
    public string? Postnummer { get; init; }
    public string? Kommunenummer { get; init; }
}