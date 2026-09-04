using Arbeidstilsynet.Common.Altinn.Correspondence;
using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Mapping;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnCorrespondenceClient(
    CorrespondenceApiClient client,
    CorrespondenceRequestAdapter requestAdapter
) : IAltinnCorrespondenceClient
{
    public async Task<AltinnCorrespondenceOverview> GetCorrespondence(
        Guid correspondenceId,
        CancellationToken cancellationToken = default
    )
    {
        var overview = await client
            .Correspondence.Api.V1.Correspondence[correspondenceId]
            .GetAsync(cancellationToken: cancellationToken);

        return overview?.ToOverview()
            ?? throw new InvalidOperationException("Failed to retrieve correspondence");
    }

    public async Task<CorrespondenceResponse> InitializeCorrespondence(
        InitializeCorrespondences request,
        List<IFormFile>? attachments = null,
        CancellationToken cancellationToken = default
    )
    {
        if (attachments is not { Count: > 0 })
        {
            var jsonResponse = await client.Correspondence.Api.V1.Correspondence.PostAsync(
                request.ToGenerated(),
                cancellationToken: cancellationToken
            );

            return jsonResponse?.ToResponse()
                ?? throw new InvalidOperationException("Failed to send correspondence");
        }

        // MultipartBody serialises its parts through a request adapter, which the generated client
        // does not surface, so the adapter is injected alongside it.
        var body = request.ToMultipartBody(requestAdapter, attachments);

        var uploadResponse = await client.Correspondence.Api.V1.Correspondence.Upload.PostAsync(
            body,
            cancellationToken: cancellationToken
        );

        return uploadResponse?.ToResponse()
            ?? throw new InvalidOperationException("Failed to send correspondence");
    }
}
