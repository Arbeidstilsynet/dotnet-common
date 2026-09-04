using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for the Altinn events API.
/// </summary>
public interface IAltinnEventsClient
{
    /// <summary>
    /// Creates an event subscription.
    /// </summary>
    Task<AltinnSubscription> Subscribe(
        AltinnSubscriptionRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes an event subscription.
    /// </summary>
    Task Unsubscribe(int subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an event subscription by its id.
    /// </summary>
    Task<AltinnSubscription?> GetAltinnSubscription(
        int subscriptionId,
        CancellationToken cancellationToken = default
    );
}
