namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;

/// <summary>
/// Represents a query for retrieving updates from Enhetsregisteret. Used for <see cref="Oppdatering"/> on both <see cref="Enhet"/> and <see cref="Underenhet"/>.
/// </summary>
public record GetOppdateringerQuery
{
    /// <summary>
    /// Show only <see cref="Oppdatering"/> from and including this timestamp. The timestamp indicates when the <see cref="Oppdatering"/> was published in the Enhetsregisteret.
    /// </summary>
    public required DateTime Dato { get; set; }

    /// <summary>
    /// Show only <see cref="Oppdatering"/> from and including this <see cref="Oppdatering.Oppdateringsid"/>. <see cref="Oppdatering.Oppdateringsid"/> is sequential and increases with each <see cref="Oppdatering"/> in the Enhetsregisteret.
    /// </summary>
    public long Oppdateringsid { get; set; } = 1;

    /// <summary>
    /// Show only <see cref="Oppdatering"/> for this <see cref="Enhet"/>/<see cref="Underenhet"/>. If none are specified, all updates will be returned.
    /// </summary>
    public string[] Organisasjonsnummer { get; set; } = [];
}
