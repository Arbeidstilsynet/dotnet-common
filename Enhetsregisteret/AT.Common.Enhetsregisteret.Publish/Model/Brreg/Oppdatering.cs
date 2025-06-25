using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;

/// <summary>
/// Representer en oppdatering av en enhet i Enhetsregisteret.
/// </summary>
public record Oppdatering
{
    /// <summary>
    /// Sekvensiell oppdateringsid for enhet.
    /// </summary>
    [JsonPropertyName("oppdateringsid")]
    public long Oppdateringsid { get; init; }

    /// <summary>
    /// Tidsstempel for når endringen på enheten ble offentliggjort i enhetsregisteret.
    /// </summary>
    [JsonPropertyName("dato")]
    public DateTime? Dato { get; init; }

    /// <summary>
    /// Organisasjonsnummeret til enheten.
    /// </summary>
    [JsonPropertyName("organisasjonsnummer")]
    public string? Organisasjonsnummer { get; init; }

    /// <summary>
    /// Typen endring som ble gjennomført på enheten.
    /// </summary>
    [JsonPropertyName("endringstype")]
    public Endringstype? Endringstype { get; init; }

    /// <summary>
    /// Lenker til relaterte ressurser i Enhetsregisteret.
    /// </summary>
    [JsonPropertyName("_links")]
    public Dictionary<string, Link>? Links { get; init; }
}
