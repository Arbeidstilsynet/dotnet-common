using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Configuration;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Mapping;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Storage;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnStorageClient(StorageApiClient client, ResolvedAltinnUrls urls)
    : IAltinnStorageClient
{
    public async Task<AltinnInstance> GetInstance(
        Guid instanceGuid,
        CancellationToken cancellationToken = default
    )
    {
        // The storage specification declares both /instances/{instanceGuid} and the older
        // /instances/{instanceOwnerPartyId}/{instanceGuid}, and marks the latter as kept only for
        // backwards compatibility. The two collide at the same position in Kiota's request-builder
        // tree, so only one can be generated, and it has to be the older form because the whole
        // data-element subtree hangs off it. The preferred endpoint is therefore addressed by URL.
        var instanceUrl = $"{urls.StorageUrl.ToString().TrimEnd('/')}/instances/{instanceGuid}";

        var instance = await client
            .Instances[0][Guid.Empty]
            .WithUrl(instanceUrl)
            .GetAsync(cancellationToken: cancellationToken);

        return instance?.ToAltinnInstance()
            ?? throw new InvalidOperationException("Failed to get instance");
    }

    public Task<AltinnInstance> GetInstance(
        AltinnCloudEvent cloudEvent,
        CancellationToken cancellationToken = default
    )
    {
        return GetInstance(cloudEvent.ToInstanceGuid(), cancellationToken);
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

    public async Task<AltinnQueryResponse<AltinnInstance>> GetInstances(
        InstanceQueryParameters queryParameters,
        CancellationToken cancellationToken = default
    )
    {
        var response = await client.Instances.GetAsync(
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
        );

        return response?.ToAltinnQueryResponse()
            ?? throw new InvalidOperationException("Failed to get instances");
    }
}
