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
    List<JsonPatchOperation> GetPatchOperations(
        IWebHostEnvironment webHostEnvironment,
        Guid dialogId,
        Guid transmissionId,
        string senderActorId,
        string receiptUrl,
        Guid instanceGuid,
        string baseUrl
    );
}
