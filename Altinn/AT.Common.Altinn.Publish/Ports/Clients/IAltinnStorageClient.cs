using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Storage.Models;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for the Altinn storage API.
/// </summary>
public interface IAltinnStorageClient
{
    /// <summary>
    /// Gets an instance by its guid.
    /// </summary>
    /// <remarks>
    /// Uses the storage API's guid-only endpoint, which Altinn prefers over the older form that
    /// also requires the instance owner party id.
    /// </remarks>
    Task<Instance> GetInstance(Guid instanceGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the instance referenced by the source URL of an Altinn cloud event.
    /// </summary>
    Task<Instance> GetInstance(
        AltinnCloudEvent cloudEvent,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the content of a data element belonging to an instance.
    /// </summary>
    Task<Stream> GetInstanceData(
        InstanceDataRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the content of a data element by its absolute URL, as found on an instance's self links.
    /// </summary>
    Task<Stream> GetInstanceData(Uri absoluteUri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single page of instances matching the given query.
    /// </summary>
    /// <remarks>
    /// Use <c>GetAllInstances</c> to page through every result.
    /// </remarks>
    Task<InstanceQueryResponse> GetInstances(
        InstanceQuery query,
        CancellationToken cancellationToken = default
    );
}
