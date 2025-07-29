using Altinn.App.Core.Infrastructure.Clients.Events;
using SubscriptionRequest = Altinn.App.Core.Infrastructure.Clients.Events.SubscriptionRequest;

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
    /// <returns>The created subscription.</returns>
    Task<Subscription> Subscribe(SubscriptionRequest request);
}
