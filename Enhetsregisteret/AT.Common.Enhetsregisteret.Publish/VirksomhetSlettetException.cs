namespace Arbeidstilsynet.Common.Enhetsregisteret;

/// <summary>
/// Exception thrown when Enhetsregisteret returns a deleted enhet/underenhet.
/// </summary>
public sealed class VirksomhetSlettetException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="VirksomhetSlettetException"/>.
    /// </summary>
    /// <param name="organisasjonsnummer">Organisasjonsnummer for slettet virksomhet.</param>
    /// <param name="navn">Navn på virksomhet, hvis tilgjengelig.</param>
    /// <param name="slettedato">Dato virksomheten ble slettet.</param>
    /// <param name="slettetVirksomhet">Det originale slettede objektet fra Enhetsregisteret.</param>
    public VirksomhetSlettetException(
        string? organisasjonsnummer,
        string? navn,
        string? slettedato,
        object? slettetVirksomhet = null
    )
        : base(CreateMessage(organisasjonsnummer, navn, slettedato))
    {
        Organisasjonsnummer = organisasjonsnummer;
        Navn = navn;
        Slettedato = slettedato;
        SlettetVirksomhet = slettetVirksomhet;
    }

    /// <summary>
    /// Organisasjonsnummer for slettet virksomhet.
    /// </summary>
    public string? Organisasjonsnummer { get; }

    /// <summary>
    /// Navn på virksomhet, hvis tilgjengelig i responsen.
    /// </summary>
    public string? Navn { get; }

    /// <summary>
    /// Dato virksomheten ble slettet.
    /// </summary>
    public string? Slettedato { get; }

    /// <summary>
    /// Det originale slettede objektet fra Enhetsregisteret, dersom kallet har tilgang til det.
    /// </summary>
    public object? SlettetVirksomhet { get; }

    private static string CreateMessage(
        string? organisasjonsnummer,
        string? navn,
        string? slettedato
    )
    {
        var orgnr = string.IsNullOrWhiteSpace(organisasjonsnummer)
            ? "<ukjent>"
            : organisasjonsnummer;
        var dato = string.IsNullOrWhiteSpace(slettedato) ? "<ukjent>" : slettedato;
        var navnPart = string.IsNullOrWhiteSpace(navn) ? string.Empty : $", navn: {navn}";

        return $"Virksomhet er slettet. Organisasjonsnummer: {orgnr}{navnPart}, slettedato: {dato}.";
    }
}
