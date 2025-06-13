using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;

/// <summary>
/// Representerer en enhet i Enhetsregisteret.
/// </summary>
/// <remarks>
/// Basert på API-dokumentasjonen: https://data.brreg.no/enhetsregisteret/api/dokumentasjon/no/swagger-ui.html
/// </remarks>
public class Enhet
{
    /// <summary>
    /// Organisasjonsnummeret til enheten.
    /// </summary>
    [JsonPropertyName("organisasjonsnummer")]
    public string? Organisasjonsnummer { get; set; }

    /// <summary>
    /// Navnet på enheten.
    /// </summary>
    [JsonPropertyName("navn")]
    public string? Navn { get; set; }

    /// <summary>
    /// Organisasjonsformen til enheten.
    /// </summary>
    [JsonPropertyName("organisasjonsform")]
    public Organisasjonsform? Organisasjonsform { get; set; }

    /// <summary>
    /// Postadressen til enheten.
    /// </summary>
    [JsonPropertyName("postadresse")]
    public Adresse? Postadresse { get; set; }

    /// <summary>
    /// Forretningsadressen til enheten.
    /// </summary>
    [JsonPropertyName("forretningsadresse")]
    public Adresse? Forretningsadresse { get; set; }

    /// <summary>
    /// Indikerer om enheten er registrert i MVA-registeret.
    /// </summary>
    [JsonPropertyName("registrertIMvaregisteret")]
    public bool? RegistrertIMvaregisteret { get; set; }

    /// <summary>
    /// Målformen til enheten.
    /// </summary>
    [JsonPropertyName("maalform")]
    public string? Maalform { get; set; }

    /// <summary>
    /// Primær næringskode for enheten.
    /// </summary>
    [JsonPropertyName("naeringskode1")]
    public Naeringskode? Naeringskode1 { get; set; }

    /// <summary>
    /// Sekundær næringskode for enheten.
    /// </summary>
    [JsonPropertyName("naeringskode2")]
    public Naeringskode? Naeringskode2 { get; set; }

    /// <summary>
    /// Tertiær næringskode for enheten.
    /// </summary>
    [JsonPropertyName("naeringskode3")]
    public Naeringskode? Naeringskode3 { get; set; }

    /// <summary>
    /// Hjelpeenhetskode for enheten.
    /// </summary>
    [JsonPropertyName("hjelpeenhetskode")]
    public Hjelpeenhetskode? Hjelpeenhetskode { get; set; }

    /// <summary>
    /// Indikerer om enheten er under avvikling.
    /// </summary>
    [JsonPropertyName("underAvvikling")]
    public bool? UnderAvvikling { get; set; }

    /// <summary>
    /// Datoen for når enheten ble satt under avvikling.
    /// </summary>
    [JsonPropertyName("underAvviklingDato")]
    public string? UnderAvviklingDato { get; set; }

    /// <summary>
    /// Indikerer om enheten er registrert i Stiftelsesregisteret.
    /// </summary>
    [JsonPropertyName("registrertIStiftelsesregisteret")]
    public bool? RegistrertIStiftelsesregisteret { get; set; }

    /// <summary>
    /// Indikerer om enheten er i konkurs.
    /// </summary>
    [JsonPropertyName("konkurs")]
    public bool? Konkurs { get; set; }

    /// <summary>
    /// Datoen for når enheten ble satt i konkurs.
    /// </summary>
    [JsonPropertyName("konkursdato")]
    public string? Konkursdato { get; set; }

    /// <summary>
    /// Datoen for når enheten ble tvangsavviklet på grunn av manglende sletting.
    /// </summary>
    [JsonPropertyName("tvangsavvikletPgaManglendeSlettingDato")]
    public string? TvangsavvikletPgaManglendeSlettingDato { get; set; }

    /// <summary>
    /// Datoen for når enheten ble tvangsopplost på grunn av manglende daglig leder.
    /// </summary>
    [JsonPropertyName("tvangsopplostPgaManglendeDagligLederDato")]
    public string? TvangsopplostPgaManglendeDagligLederDato { get; set; }

    /// <summary>
    /// Datoen for når enheten ble tvangsopplost på grunn av manglende revisor.
    /// </summary>
    [JsonPropertyName("tvangsopplostPgaManglendeRevisorDato")]
    public string? TvangsopplostPgaManglendeRevisorDato { get; set; }

    /// <summary>
    /// Datoen for når enheten ble tvangsopplost på grunn av manglende regnskap.
    /// </summary>
    [JsonPropertyName("tvangsopplostPgaManglendeRegnskapDato")]
    public string? TvangsopplostPgaManglendeRegnskapDato { get; set; }

    /// <summary>
    /// Datoen for når enheten ble tvangsopplost på grunn av mangelfullt styre.
    /// </summary>
    [JsonPropertyName("tvangsopplostPgaMangelfulltStyreDato")]
    public string? TvangsopplostPgaMangelfulltStyreDato { get; set; }

    /// <summary>
    /// Datoen for når vedtektene ble fastsatt.
    /// </summary>
    [JsonPropertyName("vedtektsdato")]
    public string? Vedtektsdato { get; set; }

    /// <summary>
    /// Faste formål for vedtektene.
    /// </summary>
    [JsonPropertyName("vedtektsfestetFormaal")]
    public string[]? VedtektsfestetFormaal { get; set; }

    /// <summary>
    /// Aktivitet for enheten.
    /// </summary>
    [JsonPropertyName("aktivitet")]
    public string[]? Aktivitet { get; set; }

    /// <summary>
    /// Indikerer om enheten er registrert i Frivillighetsregisteret.
    /// </summary>
    [JsonPropertyName("registrertIFrivillighetsregisteret")]
    public bool? RegistrertIFrivillighetsregisteret { get; set; }

    /// <summary>
    /// Datoen for når enheten ble stiftet.
    /// </summary>
    [JsonPropertyName("stiftelsesdato")]
    public string? Stiftelsesdato { get; set; }

    /// <summary>
    /// Den institusjonelle sektorkoden for enheten.
    /// </summary>
    [JsonPropertyName("institusjonellSektorkode")]
    public Institusjonellsektorkode? InstitusjonellSektorkode { get; set; }

    /// <summary>
    /// Indikerer om enheten er registrert i Forretaksregisteret.
    /// </summary>
    [JsonPropertyName("registrertIForetaksregisteret")]
    public bool? RegistrertIForetaksregisteret { get; set; }

    /// <summary>
    /// Datoen for når enheten ble registrert i Enhetsregisteret.
    /// </summary>
    [JsonPropertyName("registreringsdatoEnhetsregisteret")]
    public string? RegistreringsdatoEnhetsregisteret { get; set; }

    /// <summary>
    /// Hjemmesiden for enheten.
    /// </summary>
    [JsonPropertyName("hjemmeside")]
    public string? Hjemmeside { get; set; }

    /// <summary>
    /// Siste års regnskap for enheten.
    /// </summary>
    [JsonPropertyName("sisteInnsendteAarsregnskap")]
    public string? SisteInnsendteAarsregnskap { get; set; }

    /// <summary>
    /// Frivillig MVA registrerte beskrivelser for enheten.
    /// </summary>
    [JsonPropertyName("frivilligMvaRegistrertBeskrivelser")]
    public string[]? FrivilligMvaRegistrertBeskrivelser { get; set; }

    /// <summary>
    /// Indikerer om enheten er under tvangsavvikling eller tvangsopplosning.
    /// </summary>
    [JsonPropertyName("underTvangsavviklingEllerTvangsopplosning")]
    public bool? UnderTvangsavviklingEllerTvangsopplosning { get; set; }

    /// <summary>
    /// Antall ansatte for enheten.
    /// </summary>
    [JsonPropertyName("antallAnsatte")]
    public int? AntallAnsatte { get; set; }

    /// <summary>
    /// Indikerer om enheten har registrert antall ansatte.
    /// </summary>
    [JsonPropertyName("harRegistrertAntallAnsatte")]
    public bool? HarRegistrertAntallAnsatte { get; set; }

    /// <summary>
    /// Overordnet enhet for enheten.
    /// </summary>
    [JsonPropertyName("overordnetEnhet")]
    public string? OverordnetEnhet { get; set; }

    /// <summary>
    /// Datoen for når enheten ble slettet.
    /// </summary>
    [JsonPropertyName("slettedato")]
    public string? Slettedato { get; set; }

    /// <summary>
    /// Epostadresse for enheten.
    /// </summary>
    [JsonPropertyName("epostadresse")]
    public string? Epostadresse { get; set; }

    /// <summary>
    /// Telefonnummer for enheten.
    /// </summary>
    [JsonPropertyName("telefon")]
    public string? Telefon { get; set; }

    /// <summary>
    /// Mobilnummer for enheten.
    /// </summary>
    [JsonPropertyName("mobil")]
    public string? Mobil { get; set; }

    /// <summary>
    /// Lenker relatert til enheten.
    /// </summary>
    [JsonPropertyName("links")]
    public Dictionary<string, Link>? Links { get; set; }
}

/// <summary>
/// Representerer institusjonell sektorkode fra Enhetsregisteret.
/// </summary>
public class Institusjonellsektorkode
{
    /// <summary>
    /// Koden for den institusjonelle sektoren.
    /// </summary>
    [JsonPropertyName("kode")]
    public string? Kode { get; set; }

    /// <summary>
    /// Beskrivelse av den institusjonelle sektoren.
    /// </summary>
    [JsonPropertyName("beskrivelse")]
    public string? Beskrivelse { get; set; }
}
