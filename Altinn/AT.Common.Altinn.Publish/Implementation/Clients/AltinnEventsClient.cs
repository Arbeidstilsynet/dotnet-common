using Arbeidstilsynet.Common.Altinn.Events;
using Arbeidstilsynet.Common.Altinn.Implementation.Mapping;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnEventsClient(EventsApiClient client) : IAltinnEventsClient
{
    public async Task<AltinnSubscription?> GetAltinnSubscription(
        int subscriptionId,
        CancellationToken cancellationToken = default
    )
    {
        var subscription = await client
            .Subscriptions[subscriptionId]
            .GetAsync(cancellationToken: cancellationToken);

        return subscription?.ToAltinnSubscription()
            ?? throw new InvalidOperationException("Failed to get subscription from Altinn");
    }

    public async Task<AltinnSubscription> Subscribe(
        AltinnSubscriptionRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var subscription = await client.Subscriptions.PostAsync(
            request.ToGeneratedRequest(),
            cancellationToken: cancellationToken
        );

        return subscription?.ToAltinnSubscription()
            ?? throw new InvalidOperationException("Failed to subscribe to Altinn");
    }

    public async Task Unsubscribe(int subscriptionId, CancellationToken cancellationToken = default)
    {
        await client
            .Subscriptions[subscriptionId]
            .DeleteAsync(cancellationToken: cancellationToken);
    }
}
