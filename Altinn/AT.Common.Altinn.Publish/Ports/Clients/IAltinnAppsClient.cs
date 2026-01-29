using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for interacting directly with the altinn app instance (mutuable actions).
/// </summary>
public interface IAltinnAppsClient
{
    /// <summary>
    /// Completes an instance (marks as complete in Altinn).
    /// </summary>
    /// <param name="appSpec">Altinn app specification.</param>
    /// <param name="instanceAddress">The instance address request.</param>
    /// <returns>The completed instance mapped to our own model.</returns>
    Task<AltinnInstance> CompleteInstance(
        AltinnAppSpecification? appSpec,
        InstanceRequest instanceAddress
    );
}
