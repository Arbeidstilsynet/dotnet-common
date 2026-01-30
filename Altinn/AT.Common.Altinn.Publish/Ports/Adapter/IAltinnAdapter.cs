using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Ports.Adapter;

/// <summary>
/// Adapter interface for interacting with Altinn instances and event subscriptions.
/// </summary>
public interface IAltinnAdapter
{
    /// <summary>
    /// Gets a summary of an Altinn instance from a CloudEvent.
    /// </summary>
    /// <param name="cloudEvent">The cloud event containing instance information.</param>
    /// <returns>The <see cref="AltinnInstance"/> that the <paramref name="cloudEvent"/> pertains to.</returns>
    public Task<AltinnInstance> GetAssociatedAltinnInstance(AltinnCloudEvent cloudEvent);

    /// <summary>
    /// Returns a subscription if it exists
    /// </summary>
    /// <param name="subscriptionId">The subscription id.</param>
    /// <returns>The existing subscription, null if not found.</returns>
    public Task<AltinnSubscription?> GetAltinnSubscription(int subscriptionId);

    /// <summary>
    /// Subscribes for completed process events in Altinn.
    /// </summary>
    /// <param name="subscriptionRequestDto">The subscription request details.</param>
    /// <returns>The created subscription.</returns>
    public Task<AltinnSubscription> SubscribeForCompletedProcessEvents(
        SubscriptionRequestDto subscriptionRequestDto
    );

    /// <summary>
    /// Unsubscribes for an already registered Altinn subscription.
    /// </summary>
    /// <param name="altinnSubscription">The altinn subscription details.</param>
    /// <returns>True if it could be successfully unsubscribed, false if the ID did not exist</returns>
    public Task<bool> UnsubscribeForCompletedProcessEvents(AltinnSubscription altinnSubscription);

    /// <summary>
    /// Gets all non-completed Altinn instances for a given app.
    /// </summary>
    /// <param name="appSpec">The altinn app specification</param>
    /// <param name="processIsComplete">Whether the process is complete (default true).</param>
    /// <returns>A collection of non-completed instance summaries.</returns>
    public Task<IEnumerable<AltinnInstanceSummary>> GetNonCompletedInstances(
        AltinnAppSpecification appSpec,
        bool processIsComplete = true
    );

    /// <summary>
    /// Gets all non-completed Altinn instances for a given app.
    /// </summary>
    /// <param name="appId">The altinn application id. The rest of the specification will be default.</param>
    /// <param name="processIsComplete">Whether the process is complete (default true).</param>
    /// <returns>A collection of non-completed instance summaries.</returns>
    public Task<IEnumerable<AltinnInstanceSummary>> GetNonCompletedInstances(
        string appId,
        bool processIsComplete = true
    );

    /// <summary>
    /// Gets all non-completed Altinn instance metadata for a given app. Does not download any documents.
    /// </summary>
    /// <param name="appId">The Altinn App Id. E.g. "ulykkesvarsel" (sans "dat/")</param>
    /// <param name="processIsComplete">Whether the process is complete (default true).</param>
    /// <returns>A collection of non-completed instance summaries.</returns>
    public Task<IEnumerable<AltinnMetadata>> GetMetadataForNonCompletedInstances(
        string appId,
        bool processIsComplete = true
    );
}
