using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;

/// <summary>
/// Representerer en næringskode fra Enhetsregisteret.
/// </summary>
public record Naeringskode
{
    /// <summary>
    /// Koden for næringsgruppen.
    /// </summary>
    [JsonPropertyName("kode")]
    public string? Kode { get; init; }

    /// <summary>
    /// Beskrivelse av næringsgruppen.
    /// </summary>
    [JsonPropertyName("beskrivelse")]
    public string? Beskrivelse { get; init; }
}

/// <summary>
/// Representerer en hjelpeenhetskode fra Enhetsregisteret.
/// </summary>
public record Hjelpeenhetskode
{
    /// <summary>
    /// Koden for hjelpeenheten.
    /// </summary>
    [JsonPropertyName("kode")]
    public string? Kode { get; init; }

    /// <summary>
    /// Beskrivelse av hjelpeenheten.
    /// </summary>
    [JsonPropertyName("beskrivelse")]
    public string? Beskrivelse { get; init; }
}

/// <summary>
/// Representerer institusjonell sektorkode fra Enhetsregisteret.
/// </summary>
public record Institusjonellsektorkode
{
    /// <summary>
    /// Koden for den institusjonelle sektoren.
    /// </summary>
    [JsonPropertyName("kode")]
    public string? Kode { get; init; }

    /// <summary>
    /// Beskrivelse av den institusjonelle sektoren.
    /// </summary>
    [JsonPropertyName("beskrivelse")]
    public string? Beskrivelse { get; init; }
}