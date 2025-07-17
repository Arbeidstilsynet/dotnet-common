using Altinn.App.Core.Infrastructure.Clients.Events;
using SubscriptionRequest = Altinn.App.Core.Infrastructure.Clients.Events.SubscriptionRequest;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

public interface IAltinnEventsClient
{
    Task<Subscription> Subscribe(SubscriptionRequest request);
}
