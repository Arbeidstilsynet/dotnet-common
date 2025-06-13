using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;

/// <summary>
/// Representerer en næringskode fra Enhetsregisteret.
/// </summary>
public class Naeringskode
{
    /// <summary>
    /// Koden for næringsgruppen.
    /// </summary>
    [JsonPropertyName("kode")]
    public string? Kode { get; set; }

    /// <summary>
    /// Beskrivelse av næringsgruppen.
    /// </summary>
    [JsonPropertyName("beskrivelse")]
    public string? Beskrivelse { get; set; }
}

/// <summary>
/// Representerer en hjelpeenhetskode fra Enhetsregisteret.
/// </summary>
public class Hjelpeenhetskode
{
    /// <summary>
    /// Koden for hjelpeenheten.
    /// </summary>
    [JsonPropertyName("kode")]
    public string? Kode { get; set; }

    /// <summary>
    /// Beskrivelse av hjelpeenheten.
    /// </summary>
    [JsonPropertyName("beskrivelse")]
    public string? Beskrivelse { get; set; }
}
