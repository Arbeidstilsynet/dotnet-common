using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;

/// <summary>
/// Representerer en lenke i Enhetsregisteret.
/// </summary>
public record Link
{
    /// <summary>
    /// URL-en for selv-lenken.
    /// </summary>
    [JsonPropertyName("href")]
    public string? Href { get; init; }
}
