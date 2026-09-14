using Altinn.ApiClients.Dialogporten.ServiceOwner.Features.V1;

namespace Arbeidstilsynet.Common.AltinnApp.Model;

/// <summary>
/// Describes how a Dialogporten dialog should be resolved when handling an update
/// (endringsmelding) for a melding.
/// </summary>
public abstract record DialogResolution
{
    private DialogResolution() { }

    /// <summary>
    /// The update relates to a melding that originated in Altinn. The existing dialog
    /// should be reused by looking it up via
    /// <c>urn:altinn:instance-id:{instanceOwnerPartyId}/{MeldingId}</c>.
    /// </summary>
    /// <param name="MeldingId">The id of the melding the existing dialog belongs to.</param>
    public sealed record ReuseExisting(string MeldingId) : DialogResolution;

    /// <summary>
    /// The update relates to a melding that was not received via Altinn. A new dialog
    /// must be created using <paramref name="Request"/> before a transmission can be added.
    /// </summary>
    /// <param name="Request">The request describing the dialog to create.</param>
    public sealed record CreateNew(CreateDialog Request) : DialogResolution;
}
