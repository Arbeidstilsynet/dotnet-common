using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;

/// <summary>
/// Representerer en underenhet i Enhetsregisteret.
/// </summary>
/// <remarks>
/// Basert på API-dokumentasjonen: https://data.brreg.no/enhetsregisteret/api/dokumentasjon/no/swagger-ui.html
/// </remarks>
public class Underenhet
{
    /// <summary>
    /// Organisasjonsnummeret til underenheten.
    /// </summary>
    [JsonPropertyName("organisasjonsnummer")]
    public string? Organisasjonsnummer { get; set; }

    /// <summary>
    /// Navnet på underenheten.
    /// </summary>
    [JsonPropertyName("navn")]
    public string? Navn { get; set; }

    /// <summary>
    /// Organisasjonsformen til underenheten.
    /// </summary>
    [JsonPropertyName("organisasjonsform")]
    public Organisasjonsform? Organisasjonsform { get; set; }

    /// <summary>
    /// Postadressen til underenheten.
    /// </summary>
    [JsonPropertyName("postadresse")]
    public Adresse? Postadresse { get; set; }

    /// <summary>
    /// Den fysiske adressen til underenheten.
    /// </summary>
    [JsonPropertyName("beliggenhetsadresse")]
    public Adresse? Beliggenhetsadresse { get; set; }

    /// <summary>
    /// Indikerer om underenheten er registrert i Merverdiavgiftsregisteret.
    /// </summary>
    [JsonPropertyName("registrertIMvaregisteret")]
    public bool RegistrertIMvaregisteret { get; set; }

    /// <summary>
    /// Primær næringskode for underenheten.
    /// </summary>
    [JsonPropertyName("naeringskode1")]
    public Naeringskode? Naeringskode1 { get; set; }

    /// <summary>
    /// Sekundær næringskode for underenheten.
    /// </summary>
    [JsonPropertyName("naeringskode2")]
    public Naeringskode? Naeringskode2 { get; set; }

    /// <summary>
    /// Tertiær næringskode for underenheten.
    /// </summary>
    [JsonPropertyName("naeringskode3")]
    public Naeringskode? Naeringskode3 { get; set; }

    /// <summary>
    /// Hjelpeenhetskode for underenheten.
    /// </summary>
    [JsonPropertyName("hjelpeenhetskode")]
    public Hjelpeenhetskode? Hjelpeenhetskode { get; set; }

    /// <summary>
    /// Datoen underenheten ble registrert i Enhetsregisteret.
    /// </summary>
    [JsonPropertyName("registreringsdatoEnhetsregisteret")]
    public string? RegistreringsdatoEnhetsregisteret { get; set; }

    /// <summary>
    /// Nettstedet til underenheten.
    /// </summary>
    [JsonPropertyName("hjemmeside")]
    public string? Hjemmeside { get; set; }

    /// <summary>
    /// Beskrivelser av frivillig MVA-registrering for underenheten.
    /// </summary>
    [JsonPropertyName("frivilligMvaRegistrertBeskrivelser")]
    public string[]? FrivilligMvaRegistrertBeskrivelser { get; set; }

    /// <summary>
    /// Antall ansatte i underenheten.
    /// </summary>
    [JsonPropertyName("antallAnsatte")]
    public int? AntallAnsatte { get; set; }

    /// <summary>
    /// Indikerer om antall ansatte er registrert for underenheten.
    /// </summary>
    [JsonPropertyName("harRegistrertAntallAnsatte")]
    public bool HarRegistrertAntallAnsatte { get; set; }

    /// <summary>
    /// Organisasjonsnummeret til den overordnede enheten.
    /// </summary>
    [JsonPropertyName("overordnetEnhet")]
    public string? OverordnetEnhet { get; set; }

    /// <summary>
    /// Startdatoen for underenhetens virksomhet.
    /// </summary>
    [JsonPropertyName("oppstartsdato")]
    public string? Oppstartsdato { get; set; }

    /// <summary>
    /// Datoen for siste eierskifte av underenheten.
    /// </summary>
    [JsonPropertyName("datoEierskifte")]
    public string? DatoEierskifte { get; set; }

    /// <summary>
    /// Datoen da underenheten ble nedlagt.
    /// </summary>
    [JsonPropertyName("nedleggelsesdato")]
    public string? Nedleggelsesdato { get; set; }

    /// <summary>
    /// Epostadresse for underenheten.
    /// </summary>
    [JsonPropertyName("epostadresse")]
    public string? Epostadresse { get; set; }

    /// <summary>
    /// Telefonnummer for underenheten.
    /// </summary>
    [JsonPropertyName("telefon")]
    public string? Telefon { get; set; }

    /// <summary>
    /// Mobilnummer for underenheten.
    /// </summary>
    [JsonPropertyName("mobil")]
    public string? Mobil { get; set; }

    /// <summary>
    /// Lenker relatert til underenheten.
    /// </summary>
    [JsonPropertyName("_links")]
    public Dictionary<string, Link>? Links { get; set; }
}
