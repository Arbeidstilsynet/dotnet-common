using Altinn.ApiClients.Dialogporten.ServiceOwner.Features.V1;
using Microsoft.AspNetCore.Hosting;

namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Interface for providing patch operations to the PatchDialog task.
/// </summary>
public interface IPatchOperationsProvider
{
    /// <summary>
    /// Returns a list of patch operations to be applied to the data model.
    /// </summary>
    /// <param name="webHostEnvironment">The host environment.</param>
    /// <param name="dialogId">The id of the dialog being patched.</param>
    /// <param name="transmissionId">The id of the transmission being created.</param>
    /// <param name="senderActorId">The urn identifying the sender.</param>
    /// <param name="receiptUrl">The receipt url for the submitted instance.</param>
    /// <param name="instanceGuid">The guid part of the instance id.</param>
    /// <param name="instanceOwnerPartyId">The party id of the instance owner.</param>
    /// <param name="baseUrl">The Altinn platform base url.</param>
    List<JsonPatchOperation> GetPatchOperations(
        IWebHostEnvironment webHostEnvironment,
        Guid dialogId,
        Guid transmissionId,
        string senderActorId,
        string receiptUrl,
        Guid instanceGuid,
        int instanceOwnerPartyId,
        string baseUrl
    );
}
