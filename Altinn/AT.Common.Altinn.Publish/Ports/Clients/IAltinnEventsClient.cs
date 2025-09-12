using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for subscribing to Altinn events.
/// </summary>
public interface IAltinnEventsClient
{
    /// <summary>
    /// Subscribes to Altinn events using the specified subscription request.
    /// </summary>
    /// <param name="request">The subscription request details.</param>
    /// <returns>The created subscription mapped to our internal model.</returns>
    Task<AltinnSubscription> Subscribe(AltinnSubscriptionRequest request);
}
