using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for interacting with Altinn Storage API for instances and data.
/// </summary>
public interface IAltinnStorageClient
{
    /// <summary>
    /// Gets an instance by its address.
    /// </summary>
    /// <param name="instanceAddress">The instance address request.</param>
    /// <returns>The instance matching the address mapped to our own model.</returns>
    Task<AltinnInstance> GetInstance(InstanceRequest instanceAddress);

    /// <summary>
    /// Gets an instance from a CloudEvent.
    /// </summary>
    /// <param name="cloudEvent">The cloud event containing instance information.</param>
    /// <returns>The instance referenced by the event mapped to our own model.</returns>
    Task<AltinnInstance> GetInstance(AltinnCloudEvent cloudEvent);

    /// <summary>
    /// Gets instance data as a stream by request.
    /// </summary>
    /// <param name="request">The instance data request.</param>
    /// <returns>The data stream for the instance data.</returns>
    Task<Stream> GetInstanceData(InstanceDataRequest request);

    /// <summary>
    /// Gets instance data as a stream by absolute URI.
    /// </summary>
    /// <param name="absoluteUri">The absolute URI to the data.</param>
    /// <returns>The data stream for the instance data.</returns>
    Task<Stream> GetInstanceData(Uri absoluteUri);

    /// <summary>
    /// Gets instances matching the specified query parameters.
    /// </summary>
    /// <param name="queryParameters">The query parameters for searching instances.</param>
    /// <returns>A query response containing matching instances mapped to our own model.</returns>
    Task<AltinnQueryResponse<AltinnInstance>> GetInstances(InstanceQueryParameters queryParameters);
}
