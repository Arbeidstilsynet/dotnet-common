using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.GeoNorge;

/// <summary>
/// Represents an address in Norway, including street name, postal code, municipality number, and location.
/// </summary>
public record Address
{
    /*
     * "adressenavn": "string",
      "adressetekst": "string",
      "adressetilleggsnavn": "string",
      "adressekode": 0,
      "nummer": 0,
      "bokstav": "string",
      "kommunenummer": "string",
      "kommunenavn": "string",
      "gardsnummer": 0,
      "bruksnummer": 0,
      "festenummer": 0,
      "undernummer": 0,
      "bruksenhetsnummer": [
        "string"
      ],
      "objtype": "Vegadresse",
      "poststed": "string",
      "postnummer": "string",
      "adressetekstutenadressetilleggsnavn": "string",
      "stedfestingverifisert": true,
      "representasjonspunkt": {
        "epsg": "string",
        "lat": 0,
        "lon": 0
      },
      "oppdateringsdato": "2025-06-30T09:18:40.228Z",
      "meterDistanseTilPunkt": 0
     */
    
    /// <summary>
    /// Streetname
    /// </summary>
    [JsonPropertyName("adressenavn")]
    public string? Adressenavn { get; init; }
    /// <summary>
    /// 4 digit postals, of which the first two are the fylkenummer code.
    /// </summary>
    [JsonPropertyName("kommunenummer")]
    public string? Kommunenummer { get; init; }
    
    /// <summary>
    /// Unik identifikasjon av et postnummerområde. Tekstverdi som må bestå av 4 tall. 0340 er for eksempel gyldig, mens 340 er ikke gyldig. Postnummer som identifiserer postboksanlegg er ikke med og vil ikke gi treff.
    /// </summary>
    [JsonPropertyName("postnummer")]
    public string? Postnummer { get; init; }
    
    /// <summary>
    /// Navn på poststed i henhold til Postens egne lister
    /// </summary>
    [JsonPropertyName("poststed")]
    public string? Poststed { get; init; }
    
    /// <summary>
    /// Geolokasjon
    /// </summary>
    [JsonPropertyName("representasjonspunkt")]
    public Location? Location { get; init; }
    
    
}