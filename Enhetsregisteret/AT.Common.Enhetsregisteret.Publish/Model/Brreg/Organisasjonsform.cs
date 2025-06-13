using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;

/// <summary>
/// Representerer organisasjonsformen for en enhet i Enhetsregisteret.
/// </summary>
public class Organisasjonsform
{
    /// <summary>
    /// Lenkene tilknyttet organisasjonsformen.
    /// </summary>
    [JsonPropertyName("_links")]
    public Dictionary<string, Link>? Links { get; set; }

    /// <summary>
    /// Koden for organisasjonsformen.
    /// </summary>
    [JsonPropertyName("kode")]
    public string? Kode { get; set; }

    /// <summary>
    /// Datoen for når organisasjonsformen eventuelt er utgått.
    /// </summary>
    [JsonPropertyName("utgaatt")]
    public string? Utgaatt { get; set; }

    /// <summary>
    /// Beskrivelsen av organisasjonsformen.
    /// </summary>
    [JsonPropertyName("beskrivelse")]
    public string? Beskrivelse { get; set; }
}
