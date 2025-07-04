namespace Arbeidstilsynet.Common.GeoNorge.Model.Request;

/// <summary>
/// Represents a query for searching addresses based on a text search term.
/// </summary>
public record TextSearchQuery
{
    /// <summary>
    /// The text to search for (e.g., street name, place name, or address).
    /// </summary>
    public required string SearchTerm { get; init; }

    /// <summary>
    /// Indicates whether to perform a fuzzy search.
    /// </summary>
    public bool FuzzySearch { get; init; } = false;

    /// <summary>
    /// Optional address name to filter the search results by a specific address name.
    /// </summary>
    public string? Adressenavn { get; init; }

    /// <summary>
    /// Optional additional address name to filter the search results by a specific additional address name (e.g., farm name, building name).
    /// </summary>
    public string? Poststed { get; init; }

    /// <summary>
    /// Optional postal code to filter the search results by a specific postal code.
    /// </summary>
    public string? Postnummer { get; init; }

    /// <summary>
    /// Optional municipality number to filter the search results by a specific municipality number (4 digits, first two are the county code).
    /// </summary>
    public string? Kommunenummer { get; init; }
}
