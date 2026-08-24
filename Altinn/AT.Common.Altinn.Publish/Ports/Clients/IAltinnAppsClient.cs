using Arbeidstilsynet.Common.Altinn.Apps.Models;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for the Altinn apps API.
/// </summary>
public interface IAltinnAppsClient
{
    /// <summary>
    /// Marks an instance as complete on behalf of the configured organisation.
    /// </summary>
    Task<Instance> CompleteInstance(
        string appId,
        InstanceRequest instanceAddress,
        CancellationToken cancellationToken = default
    );
}
