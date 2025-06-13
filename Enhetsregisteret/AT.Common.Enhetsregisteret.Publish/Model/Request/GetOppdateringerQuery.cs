namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;

/// <summary>
/// Representerer en søkespørring for å hente oppdateringer fra Enhetsregisteret. Brukes både for oppdateringer på enheter og underenheter.
/// </summary>
public record GetOppdateringerQuery
{
    /// <summary>
    /// Vis bare oppdateringer fra og med dette tidsstempelet. Tidsstempelet indikerer når oppdateringen ble offentliggjort i Enhetsregisteret.
    /// </summary>
    public required DateTime Dato { get; set; }

    /// <summary>
    /// Vis bare oppdateringer fra og med denne oppdateringsid-en. Oppdateringsid-en er sekvensiell og øker med hver oppdatering i Enhetsregisteret.
    /// </summary>
    public long Oppdateringsid { get; set; } = 1;

    /// <summary>
    /// Vis bare oppdateringer for disse organisasjonsnumrene. Hvis ingen organisasjonsnumre er spesifisert, vil alle oppdateringer bli returnert.
    /// </summary>
    public string[] Organisasjonsnummer { get; set; } = [];
}
