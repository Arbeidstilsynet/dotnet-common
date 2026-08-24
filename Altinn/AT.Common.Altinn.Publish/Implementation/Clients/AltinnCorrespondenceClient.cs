using Arbeidstilsynet.Common.Altinn.Correspondence;
using Arbeidstilsynet.Common.Altinn.Correspondence.Models;
using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnCorrespondenceClient(
    CorrespondenceApiClient client,
    CorrespondenceRequestAdapter requestAdapter
) : IAltinnCorrespondenceClient
{
    public async Task<CorrespondenceOverviewExt> GetCorrespondence(
        Guid correspondenceId,
        CancellationToken cancellationToken = default
    )
    {
        return await client
                .Correspondence.Api.V1.Correspondence[correspondenceId]
                .GetAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve correspondence");
    }

    public async Task<InitializeCorrespondencesResponseExt> InitializeCorrespondence(
        InitializeCorrespondencesExt request,
        List<IFormFile>? attachments = null,
        CancellationToken cancellationToken = default
    )
    {
        if (attachments is not { Count: > 0 })
        {
            return await client.Correspondence.Api.V1.Correspondence.PostAsync(
                    request,
                    cancellationToken: cancellationToken
                ) ?? throw new InvalidOperationException("Failed to send correspondence");
        }

        // MultipartBody serialises its parts through a request adapter, which the generated client
        // does not surface, so the adapter is injected alongside it.
        var body = request.ToMultipartBody(requestAdapter, attachments);

        return await client.Correspondence.Api.V1.Correspondence.Upload.PostAsync(
                body,
                cancellationToken: cancellationToken
            ) ?? throw new InvalidOperationException("Failed to send correspondence");
    }
}
