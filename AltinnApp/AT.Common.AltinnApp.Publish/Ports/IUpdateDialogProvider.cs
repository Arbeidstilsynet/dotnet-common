using Altinn.ApiClients.Dialogporten.ServiceOwner.Features.V1;

namespace Arbeidstilsynet.Common.AltinnApp.Ports;

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

/// <summary>
/// Interface for supplying the update-specific dialog logic to the UpdateDialog task.
/// </summary>
/// <typeparam name="T">The structured data model type.</typeparam>
public interface IUpdateDialogProvider<T>
    where T : class
{
    /// <summary>
    /// Decides whether an existing Altinn dialog should be reused or a new dialog should be
    /// created for the given model.
    /// </summary>
    /// <param name="model">The submitted skjema model.</param>
    /// <param name="dialogId">
    /// A pre-generated dialog id (UUIDv7) to use when a new dialog is created. Use this as the
    /// <c>Id</c> of the returned <see cref="DialogResolution.CreateNew"/> request so the task can
    /// reference the created dialog afterwards.
    /// </param>
    /// <returns>The resolution describing how to obtain the dialog.</returns>
    DialogResolution Resolve(T model, Guid dialogId);
}
