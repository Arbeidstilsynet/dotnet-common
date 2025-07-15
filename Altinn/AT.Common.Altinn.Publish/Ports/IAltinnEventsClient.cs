using Altinn.App.Core.Infrastructure.Clients.Events;
using SubscriptionRequest = Altinn.App.Core.Infrastructure.Clients.Events.SubscriptionRequest;

namespace Arbeidstilsynet.Common.Altinn.Ports;

public interface IAltinnEventsClient
{
    Task<Subscription> Subscribe(SubscriptionRequest request);
}
