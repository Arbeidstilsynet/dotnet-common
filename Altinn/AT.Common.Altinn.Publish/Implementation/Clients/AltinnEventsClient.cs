using Arbeidstilsynet.Common.Altinn.Events;
using Arbeidstilsynet.Common.Altinn.Events.Models;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnEventsClient(EventsApiClient client) : IAltinnEventsClient
{
    public async Task<Subscription> GetAltinnSubscription(
        int subscriptionId,
        CancellationToken cancellationToken = default
    )
    {
        return await client.Subscriptions[subscriptionId].GetAsync(
                cancellationToken: cancellationToken
            ) ?? throw new InvalidOperationException("Failed to get subscription from Altinn");
    }

    public async Task<Subscription> Subscribe(
        SubscriptionRequestModel request,
        CancellationToken cancellationToken = default
    )
    {
        return await client.Subscriptions.PostAsync(request, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to subscribe to Altinn");
    }

    public async Task Unsubscribe(
        int subscriptionId,
        CancellationToken cancellationToken = default
    )
    {
        await client.Subscriptions[subscriptionId].DeleteAsync(
            cancellationToken: cancellationToken
        );
    }
}
