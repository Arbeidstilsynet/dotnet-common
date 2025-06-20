using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;

/// <summary>
/// Representerer en adresse i Enhetsregisteret.
/// </summary>
public class Adresse
{
    /// <summary>
    /// Kommunen adressen tilhører.
    /// </summary>
    [JsonPropertyName("kommune")]
    public string? Kommune { get; set; }

    /// <summary>
    /// Landkoden for adressen.
    /// </summary>
    [JsonPropertyName("landkode")]
    public string? Landkode { get; set; }

    /// <summary>
    /// Postnummeret for adressen.
    /// </summary>
    [JsonPropertyName("postnummer")]
    public string? Postnummer { get; set; }

    /// <summary>
    /// Gateadressen, kan inneholde flere linjer.
    /// </summary>
    [JsonPropertyName("adresse")]
    public string[]? Gateadresse { get; set; }

    /// <summary>
    /// Landet adressen tilhører.
    /// </summary>
    [JsonPropertyName("land")]
    public string? Land { get; set; }

    /// <summary>
    /// Kommunenummeret for adressen.
    /// </summary>
    [JsonPropertyName("kommunenummer")]
    public string? Kommunenummer { get; set; }

    /// <summary>
    /// Poststedet for adressen.
    /// </summary>
    [JsonPropertyName("poststed")]
    public string? Poststed { get; set; }
}
