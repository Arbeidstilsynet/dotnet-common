using Altinn.App.Core.Infrastructure.Clients.Events;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using SubscriptionRequest = Altinn.App.Core.Infrastructure.Clients.Events.SubscriptionRequest;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for interacting directly with the altinn app instance (mutuable actions).
/// </summary>
public interface IAltinnAppsClient
{
    /// <summary>
    /// Completes an instance (marks as complete in Altinn).
    /// </summary>
    /// <param name="appId">The appId which should be queried</param>
    /// <param name="instanceAddress">The instance address request.</param>
    /// <returns>The completed instance.</returns>
    Task<Instance> CompleteInstance(string appId, InstanceRequest instanceAddress);
}
