using Arbeidstilsynet.Common.Enhetsregisteret.Models;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;

/// <summary>
/// Represents a query for retrieving updates from Enhetsregisteret. Used for oppdateringer on both <see cref="Enhet"/> and <see cref="Underenhet"/>.
/// </summary>
public record GetOppdateringerQuery
{
    /// <summary>
    /// Show only oppdateringer from and including this timestamp. The timestamp indicates when the oppdatering was published in the Enhetsregisteret.
    /// </summary>
    public required DateTime Dato { get; set; }

    /// <summary>
    /// Show only oppdateringer from and including this oppdateringsid. The oppdateringsid is sequential and increases with each oppdatering in the Enhetsregisteret.
    /// </summary>
    public long Oppdateringsid { get; set; } = 1;

    /// <summary>
    /// Show only oppdateringer for this <see cref="Enhet"/>/<see cref="Underenhet"/>. If none are specified, all updates will be returned.
    /// </summary>
    public string[] Organisasjonsnummer { get; set; } = [];
}
