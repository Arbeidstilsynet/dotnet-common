using Arbeidstilsynet.Common.Altinn.DependencyInjection;
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
    /// <param name="appConfig">Optional Altinn app configuration.</param>
    /// <returns>The summary of the Altinn instance.</returns>
    public Task<AltinnInstanceSummary> GetSummary(
        AltinnCloudEvent cloudEvent,
        AltinnAppConfiguration? appConfig = null
    );

    /// <summary>
    /// Subscribes for completed process events in Altinn.
    /// </summary>
    /// <param name="subscriptionRequestDto">The subscription request details.</param>
    /// <returns>The created subscription.</returns>
    public Task<AltinnSubscription> SubscribeForCompletedProcessEvents(
        SubscriptionRequestDto subscriptionRequestDto
    );

    /// <summary>
    /// Gets all non-completed Altinn instances for a given app.
    /// </summary>
    /// <param name="appId">The Altinn app ID. E.g. "ulykkesvarsel" will be treated as "dat/ulykkesvarsel"</param>
    /// <param name="ProcessIsComplete">Whether the process is complete (default true).</param>
    /// <param name="ExcludeConfirmedBy">Exclude instances which are already confirmed by the specified org identifier. Default: "dat"</param>
    /// <param name="appConfig">Optional Altinn app configuration.</param>
    /// <returns>A collection of non-completed instance summaries.</returns>
    public Task<IEnumerable<AltinnInstanceSummary>> GetNonCompletedInstances(
        string appId,
        bool ProcessIsComplete = true,
        string? ExcludeConfirmedBy = DependencyInjectionExtensions.AltinnOrgIdentifier,
        AltinnAppConfiguration? appConfig = null
    );

    /// <summary>
    /// Gets all non-completed Altinn instance metadata for a given app. Does not download any attachments.
    /// </summary>
    /// <param name="appId">The Altinn app ID. E.g. "ulykkesvarsel" will be treated as "dat/ulykkesvarsel"</param>
    /// <param name="ProcessIsComplete">Whether the process is complete (default true).</param>
    /// <param name="ExcludeConfirmedBy">Exclude instances which are already confirmed by the specified org identifier. Default: "dat"</param>
    /// <returns>A collection of non-completed instance summaries.</returns>
    public Task<IEnumerable<AltinnMetadata>> GetMetadataForNonCompletedInstances(
        string appId,
        bool ProcessIsComplete = true,
        string? ExcludeConfirmedBy = DependencyInjectionExtensions.AltinnOrgIdentifier
    );
}
