using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Storage;
using Arbeidstilsynet.Common.Altinn.Storage.Models;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnStorageClient(StorageApiClient client) : IAltinnStorageClient
{
    public async Task<Instance> GetInstance(
        InstanceRequest instanceAddress,
        CancellationToken cancellationToken = default
    )
    {
        return await client
                .Instances[instanceAddress.GetInstanceOwnerPartyId()][instanceAddress.InstanceGuid]
                .GetAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to get instance");
    }

    public Task<Instance> GetInstance(
        AltinnCloudEvent cloudEvent,
        CancellationToken cancellationToken = default
    )
    {
        return GetInstance(cloudEvent.ToInstanceRequest(), cancellationToken);
    }

    public async Task<Stream> GetInstanceData(
        InstanceDataRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return await client
                .Instances[request.InstanceRequest.GetInstanceOwnerPartyId()][
                    request.InstanceRequest.InstanceGuid
                ]
                .Data[request.DataId]
                .GetAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to get instance data");
    }

    public async Task<Stream> GetInstanceData(
        Uri absoluteUri,
        CancellationToken cancellationToken = default
    )
    {
        // The path parameters are irrelevant here: WithUrl replaces the whole URL. Data elements
        // are commonly addressed by the absolute self-link returned on an instance.
        return await client
                .Instances[0][Guid.Empty]
                .Data[Guid.Empty]
                .WithUrl(absoluteUri.ToString())
                .GetAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to get instance data");
    }

    public async Task<InstanceQueryResponse> GetInstances(
        InstanceQueryParameters queryParameters,
        CancellationToken cancellationToken = default
    )
    {
        return await client.Instances.GetAsync(
                request =>
                {
                    queryParameters.ApplyTo(request.QueryParameters);

                    // Kiota omits header parameters from the generated query-parameter class, so
                    // the instance owner identifier has to be applied to the request directly.
                    if (queryParameters.InstanceOwnerIdentifier is { Length: > 0 } identifier)
                    {
                        request.Headers.Add(
                            InstanceQueryParameters.InstanceOwnerIdentifierHeaderName,
                            identifier
                        );
                    }
                },
                cancellationToken
            ) ?? throw new InvalidOperationException("Failed to get instances");
    }
}
