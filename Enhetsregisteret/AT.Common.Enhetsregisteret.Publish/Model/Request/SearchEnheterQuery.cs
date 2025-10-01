using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;

/// <summary>
/// Represents a search query for finding <see cref="Enhet"/> and <see cref="Underenhet"/> in the Enhetsregisteret.
/// </summary>
public record SearchEnheterQuery
{
    /// <summary>
    /// Sort order for the results.
    /// </summary>
    public enum Sort
    {
        /// <summary>
        /// Sort the results in ascending order.
        /// </summary>
        Asc,

        /// <summary>
        /// Sort the results in descending order.
        /// </summary>
        Desc,
    }

    /// <summary>
    /// Search text for finding <see cref="Enhet"/> and <see cref="Underenhet"/> based on name.
    /// <remarks>
    /// Maximum of 180 characters.
    /// </remarks>
    /// </summary>
    public string? Navn { get; set; }

    /// <summary>
    /// Only return <see cref="Enhet"/>/<see cref="Underenhet"/> with these organizational numbers. If none are specified, any <see cref="Enhet"/>/<see cref="Underenhet"/> will be returned.
    /// </summary>
    public string[] Organisasjonsnummer { get; set; } = [];

    /// <summary>
    /// Only return <see cref="Enhet"/>/<see cref="Underenhet"/> where the "hovedenhet" has this organizational number.
    /// </summary>
    public string? OverordnetEnhetOrganisasjonsnummer { get; set; }

    /// <summary>
    /// Organizational form of the <see cref="Enhet"/>/<see cref="Underenhet"/>.
    /// </summary>
    public string[] Organisasjonsform { get; set; } = [];

    /// <summary>
    /// This parameter (navnMetodeForSoek) is not very well documented, but presumably the words in <see cref="Navn"/> will be ANDed, rather than ORed. Defaults to false.
    /// </summary>
    public bool StrictSearch { get; set; } = false;

    /// <summary>
    /// Sort order for the results.
    /// </summary>
    public Sort SortDirection { get; set; } = Sort.Asc;

    /// <summary>
    /// Field to sort the results by. Documentation for possible values can be found in the API documentation. https://data.brreg.no/enhetsregisteret/api/dokumentasjon/no/index.html#tag/Enheter
    /// </summary>
    public string? SortBy { get; set; }
}
