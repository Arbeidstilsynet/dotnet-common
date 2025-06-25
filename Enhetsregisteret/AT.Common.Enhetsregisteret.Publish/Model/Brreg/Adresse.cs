using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;

/// <summary>
/// Representerer en adresse i Enhetsregisteret.
/// </summary>
public record Adresse
{
    /// <summary>
    /// Kommunen adressen tilhører.
    /// </summary>
    [JsonPropertyName("kommune")]
    public string? Kommune { get; init; }

    /// <summary>
    /// Landkoden for adressen.
    /// </summary>
    [JsonPropertyName("landkode")]
    public string? Landkode { get; init; }

    /// <summary>
    /// Postnummeret for adressen.
    /// </summary>
    [JsonPropertyName("postnummer")]
    public string? Postnummer { get; init; }

    /// <summary>
    /// Gateadressen, kan inneholde flere linjer.
    /// </summary>
    [JsonPropertyName("adresse")]
    public string[]? Gateadresse { get; init; }

    /// <summary>
    /// Landet adressen tilhører.
    /// </summary>
    [JsonPropertyName("land")]
    public string? Land { get; init; }

    /// <summary>
    /// Kommunenummeret for adressen.
    /// </summary>
    [JsonPropertyName("kommunenummer")]
    public string? Kommunenummer { get; init; }

    /// <summary>
    /// Poststedet for adressen.
    /// </summary>
    [JsonPropertyName("poststed")]
    public string? Poststed { get; init; }
}
