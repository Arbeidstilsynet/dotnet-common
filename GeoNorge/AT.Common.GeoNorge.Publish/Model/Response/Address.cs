using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.GeoNorge.Model.Response;

/// <summary>
/// Represents an address in Norway, including street name, postal code, municipality number, and location.
/// </summary>
public record Address
{    
    /// <summary>
    /// Street name.
    /// </summary>
    [JsonPropertyName("adressenavn")]
    public string? Adressenavn { get; init; }

    /// <summary>
    /// Address text (full address as a string).
    /// </summary>
    [JsonPropertyName("adressetekst")]
    public string? Adressetekst { get; init; }

    /// <summary>
    /// Additional address name (e.g., farm name, building name).
    /// </summary>
    [JsonPropertyName("adressetilleggsnavn")]
    public string? Adressetilleggsnavn { get; init; }

    /// <summary>
    /// Address code (unique code for the address).
    /// </summary>
    [JsonPropertyName("adressekode")]
    public int? Adressekode { get; init; }

    /// <summary>
    /// House or property number.
    /// </summary>
    [JsonPropertyName("nummer")]
    public int? Nummer { get; init; }

    /// <summary>
    /// Letter suffix for the house or property number.
    /// </summary>
    [JsonPropertyName("bokstav")]
    public string? Bokstav { get; init; }

    /// <summary>
    /// Municipality number (4 digits, first two are the county code).
    /// </summary>
    [JsonPropertyName("kommunenummer")]
    public string? Kommunenummer { get; init; }

    /// <summary>
    /// Municipality name.
    /// </summary>
    [JsonPropertyName("kommunenavn")]
    public string? Kommunenavn { get; init; }

    /// <summary>
    /// Farm number (gårdsnummer).
    /// </summary>
    [JsonPropertyName("gardsnummer")]
    public int? Gardsnummer { get; init; }

    /// <summary>
    /// Usage number (bruksnummer).
    /// </summary>
    [JsonPropertyName("bruksnummer")]
    public int? Bruksnummer { get; init; }

    /// <summary>
    /// Lease number (festenummer).
    /// </summary>
    [JsonPropertyName("festenummer")]
    public int? Festenummer { get; init; }

    /// <summary>
    /// Subnumber (undernummer).
    /// </summary>
    [JsonPropertyName("undernummer")]
    public int? Undernummer { get; init; }

    /// <summary>
    /// List of unit numbers (bruksenhetsnummer).
    /// </summary>
    [JsonPropertyName("bruksenhetsnummer")]
    public List<string>? Bruksenhetsnummer { get; init; }

    /// <summary>
    /// Object type (e.g., "Vegadresse").
    /// </summary>
    [JsonPropertyName("objtype")]
    public string? Objtype { get; init; }

    /// <summary>
    /// Name of the post town according to the Norwegian postal service.
    /// </summary>
    [JsonPropertyName("poststed")]
    public string? Poststed { get; init; }

    /// <summary>
    /// Postal code (4 digits).
    /// </summary>
    [JsonPropertyName("postnummer")]
    public string? Postnummer { get; init; }

    /// <summary>
    /// Address text without the additional address name.
    /// </summary>
    [JsonPropertyName("adressetekstutenadressetilleggsnavn")]
    public string? AdressetekstUtenAdressetilleggsnavn { get; init; }

    /// <summary>
    /// Whether the location is verified.
    /// </summary>
    [JsonPropertyName("stedfestingverifisert")]
    public bool? StedfestingVerifisert { get; init; }

    /// <summary>
    /// Geolocation representation point.
    /// </summary>
    [JsonPropertyName("representasjonspunkt")]
    public Location? Location { get; init; }

    /// <summary>
    /// Date of last update.
    /// </summary>
    [JsonPropertyName("oppdateringsdato")]
    public DateTimeOffset? Oppdateringsdato { get; init; }

    /// <summary>
    /// Distance in meters to the point.
    /// </summary>
    [JsonPropertyName("meterDistanseTilPunkt")]
    public double? MeterDistanseTilPunkt { get; init; }
}