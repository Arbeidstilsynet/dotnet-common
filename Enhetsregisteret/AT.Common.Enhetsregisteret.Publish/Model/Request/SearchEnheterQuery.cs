namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;

/// <summary>
/// Representerer en søkespørring for å finne enheter i Enhetsregisteret. Brukes både på /underenheter og /enheter
/// </summary>
public record SearchEnheterQuery
{
    /// <summary>
    /// Sorteringsrekkefølge for resultatene.
    /// </summary>
    public enum Sort
    {
        /// <summary>
        /// Sorter resultatene i stigende rekkefølge.
        /// </summary>
        Asc,

        /// <summary>
        /// Sorter resultatene i synkende rekkefølge.
        /// </summary>
        Desc,
    }

    /// <summary>
    /// Søketekst for å finne enheter basert på navn.
    /// </summary>
    public string? Navn { get; set; }

    /// <summary>
    /// Organisasjonsnumre for enheter.
    /// </summary>
    public string[] Organisasjonsnummer { get; set; } = [];

    /// <summary>
    /// Organisasjonsnummeret til hovedenheten.
    /// </summary>
    public string? OverordnetEnhetOrganisasjonsnummer { get; set; }

    /// <summary>
    /// Organisasjonsformen til enheten.
    /// </summary>
    public string[] Organisasjonsform { get; set; } = [];

    /// <summary>
    /// Sorteringsrekkefølge for resultatene.
    /// </summary>
    public Sort SortDirection { get; set; } = Sort.Asc;

    /// <summary>
    /// Feltet som resultatene skal sorteres etter. Dokumentasjon for mulige verdier finnes i API-dokumentasjonen. https://data.brreg.no/enhetsregisteret/api/dokumentasjon/no/index.html#tag/Enheter
    /// </summary>
    public string? SortBy { get; set; }
}
