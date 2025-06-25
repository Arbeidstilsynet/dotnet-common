using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;

/// <summary>
/// Representerer en enhet i Enhetsregisteret.
/// </summary>
/// <remarks>
/// Basert på API-dokumentasjonen: https://data.brreg.no/enhetsregisteret/api/dokumentasjon/no/swagger-ui.html
/// </remarks>
public record Enhet
{
    /// <summary>
    /// Organisasjonsnummeret til enheten.
    /// </summary>
    [JsonPropertyName("organisasjonsnummer")]
    public string? Organisasjonsnummer { get; init; }

    /// <summary>
    /// Navnet på enheten.
    /// </summary>
    [JsonPropertyName("navn")]
    public string? Navn { get; init; }

    /// <summary>
    /// Organisasjonsformen til enheten.
    /// </summary>
    [JsonPropertyName("organisasjonsform")]
    public Organisasjonsform? Organisasjonsform { get; init; }

    /// <summary>
    /// Postadressen til enheten.
    /// </summary>
    [JsonPropertyName("postadresse")]
    public Adresse? Postadresse { get; init; }

    /// <summary>
    /// Forretningsadressen til enheten.
    /// </summary>
    [JsonPropertyName("forretningsadresse")]
    public Adresse? Forretningsadresse { get; init; }

    /// <summary>
    /// Indikerer om enheten er registrert i MVA-registeret.
    /// </summary>
    [JsonPropertyName("registrertIMvaregisteret")]
    public bool? RegistrertIMvaregisteret { get; init; }

    /// <summary>
    /// Målformen til enheten.
    /// </summary>
    [JsonPropertyName("maalform")]
    public string? Maalform { get; init; }

    /// <summary>
    /// Primær næringskode for enheten.
    /// </summary>
    [JsonPropertyName("naeringskode1")]
    public Naeringskode? Naeringskode1 { get; init; }

    /// <summary>
    /// Sekundær næringskode for enheten.
    /// </summary>
    [JsonPropertyName("naeringskode2")]
    public Naeringskode? Naeringskode2 { get; init; }

    /// <summary>
    /// Tertiær næringskode for enheten.
    /// </summary>
    [JsonPropertyName("naeringskode3")]
    public Naeringskode? Naeringskode3 { get; init; }

    /// <summary>
    /// Hjelpeenhetskode for enheten.
    /// </summary>
    [JsonPropertyName("hjelpeenhetskode")]
    public Hjelpeenhetskode? Hjelpeenhetskode { get; init; }

    /// <summary>
    /// Indikerer om enheten er under avvikling.
    /// </summary>
    [JsonPropertyName("underAvvikling")]
    public bool? UnderAvvikling { get; init; }

    /// <summary>
    /// Datoen for når enheten ble satt under avvikling.
    /// </summary>
    [JsonPropertyName("underAvviklingDato")]
    public string? UnderAvviklingDato { get; init; }

    /// <summary>
    /// Indikerer om enheten er registrert i Stiftelsesregisteret.
    /// </summary>
    [JsonPropertyName("registrertIStiftelsesregisteret")]
    public bool? RegistrertIStiftelsesregisteret { get; init; }

    /// <summary>
    /// Indikerer om enheten er i konkurs.
    /// </summary>
    [JsonPropertyName("konkurs")]
    public bool? Konkurs { get; init; }

    /// <summary>
    /// Datoen for når enheten ble satt i konkurs.
    /// </summary>
    [JsonPropertyName("konkursdato")]
    public string? Konkursdato { get; init; }

    /// <summary>
    /// Datoen for når enheten ble tvangsavviklet på grunn av manglende sletting.
    /// </summary>
    [JsonPropertyName("tvangsavvikletPgaManglendeSlettingDato")]
    public string? TvangsavvikletPgaManglendeSlettingDato { get; init; }

    /// <summary>
    /// Datoen for når enheten ble tvangsopplost på grunn av manglende daglig leder.
    /// </summary>
    [JsonPropertyName("tvangsopplostPgaManglendeDagligLederDato")]
    public string? TvangsopplostPgaManglendeDagligLederDato { get; init; }

    /// <summary>
    /// Datoen for når enheten ble tvangsopplost på grunn av manglende revisor.
    /// </summary>
    [JsonPropertyName("tvangsopplostPgaManglendeRevisorDato")]
    public string? TvangsopplostPgaManglendeRevisorDato { get; init; }

    /// <summary>
    /// Datoen for når enheten ble tvangsopplost på grunn av manglende regnskap.
    /// </summary>
    [JsonPropertyName("tvangsopplostPgaManglendeRegnskapDato")]
    public string? TvangsopplostPgaManglendeRegnskapDato { get; init; }

    /// <summary>
    /// Datoen for når enheten ble tvangsopplost på grunn av mangelfullt styre.
    /// </summary>
    [JsonPropertyName("tvangsopplostPgaMangelfulltStyreDato")]
    public string? TvangsopplostPgaMangelfulltStyreDato { get; init; }

    /// <summary>
    /// Datoen for når vedtektene ble fastsatt.
    /// </summary>
    [JsonPropertyName("vedtektsdato")]
    public string? Vedtektsdato { get; init; }

    /// <summary>
    /// Faste formål for vedtektene.
    /// </summary>
    [JsonPropertyName("vedtektsfestetFormaal")]
    public string[]? VedtektsfestetFormaal { get; init; }

    /// <summary>
    /// Aktivitet for enheten.
    /// </summary>
    [JsonPropertyName("aktivitet")]
    public string[]? Aktivitet { get; init; }

    /// <summary>
    /// Indikerer om enheten er registrert i Frivillighetsregisteret.
    /// </summary>
    [JsonPropertyName("registrertIFrivillighetsregisteret")]
    public bool? RegistrertIFrivillighetsregisteret { get; init; }

    /// <summary>
    /// Datoen for når enheten ble stiftet.
    /// </summary>
    [JsonPropertyName("stiftelsesdato")]
    public string? Stiftelsesdato { get; init; }

    /// <summary>
    /// Den institusjonelle sektorkoden for enheten.
    /// </summary>
    [JsonPropertyName("institusjonellSektorkode")]
    public Institusjonellsektorkode? InstitusjonellSektorkode { get; init; }

    /// <summary>
    /// Indikerer om enheten er registrert i Forretaksregisteret.
    /// </summary>
    [JsonPropertyName("registrertIForetaksregisteret")]
    public bool? RegistrertIForetaksregisteret { get; init; }

    /// <summary>
    /// Datoen for når enheten ble registrert i Enhetsregisteret.
    /// </summary>
    [JsonPropertyName("registreringsdatoEnhetsregisteret")]
    public string? RegistreringsdatoEnhetsregisteret { get; init; }

    /// <summary>
    /// Hjemmesiden for enheten.
    /// </summary>
    [JsonPropertyName("hjemmeside")]
    public string? Hjemmeside { get; init; }

    /// <summary>
    /// Siste års regnskap for enheten.
    /// </summary>
    [JsonPropertyName("sisteInnsendteAarsregnskap")]
    public string? SisteInnsendteAarsregnskap { get; init; }

    /// <summary>
    /// Frivillig MVA registrerte beskrivelser for enheten.
    /// </summary>
    [JsonPropertyName("frivilligMvaRegistrertBeskrivelser")]
    public string[]? FrivilligMvaRegistrertBeskrivelser { get; init; }

    /// <summary>
    /// Indikerer om enheten er under tvangsavvikling eller tvangsopplosning.
    /// </summary>
    [JsonPropertyName("underTvangsavviklingEllerTvangsopplosning")]
    public bool? UnderTvangsavviklingEllerTvangsopplosning { get; init; }

    /// <summary>
    /// Antall ansatte for enheten.
    /// </summary>
    [JsonPropertyName("antallAnsatte")]
    public int? AntallAnsatte { get; init; }

    /// <summary>
    /// Indikerer om enheten har registrert antall ansatte.
    /// </summary>
    [JsonPropertyName("harRegistrertAntallAnsatte")]
    public bool? HarRegistrertAntallAnsatte { get; init; }

    /// <summary>
    /// Overordnet enhet for enheten.
    /// </summary>
    [JsonPropertyName("overordnetEnhet")]
    public string? OverordnetEnhet { get; init; }

    /// <summary>
    /// Datoen for når enheten ble slettet.
    /// </summary>
    [JsonPropertyName("slettedato")]
    public string? Slettedato { get; init; }

    /// <summary>
    /// Epostadresse for enheten.
    /// </summary>
    [JsonPropertyName("epostadresse")]
    public string? Epostadresse { get; init; }

    /// <summary>
    /// Telefonnummer for enheten.
    /// </summary>
    [JsonPropertyName("telefon")]
    public string? Telefon { get; init; }

    /// <summary>
    /// Mobilnummer for enheten.
    /// </summary>
    [JsonPropertyName("mobil")]
    public string? Mobil { get; init; }

    /// <summary>
    /// Lenker relatert til enheten.
    /// </summary>
    [JsonPropertyName("_links")]
    public Dictionary<string, Link>? Links { get; init; }
}

