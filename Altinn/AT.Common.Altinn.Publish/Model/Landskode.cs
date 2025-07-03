namespace Arbeidstilsynet.Common.Altinn.Model;

/// <summary>
/// Represents a country and its international dialing code.
/// </summary>
/// <param name="Land">The country name in English, e.g. "Norway".</param>
/// <param name="Kode">International dialing code, e.g. "+47" for Norway.</param>
public record Landskode(string Land, string Kode);