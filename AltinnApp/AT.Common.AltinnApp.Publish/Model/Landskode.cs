using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.AltinnApp.Model;

/// <summary>
/// Represents a country and its international dialing code.
/// </summary>
/// <param name="Land">The country name in English, e.g. "Norway".</param>
/// <param name="Kode">International dialing code, e.g. "+47" for Norway.</param>
/// <param name="Alpha2">ISO 3166-1 alpha-2, e.g. "NO" for Norway.</param>
/// <param name="Alpha3">ISO 3166-1 alpha-3, e.g. "NOR" for Norway.</param>
public record Landskode(
    [property: JsonPropertyName("land")] string Land,
    [property: JsonPropertyName("kode")] string Kode,
    [property: JsonPropertyName("alpha2")] string Alpha2,
    [property: JsonPropertyName("alpha3")] string Alpha3
);
