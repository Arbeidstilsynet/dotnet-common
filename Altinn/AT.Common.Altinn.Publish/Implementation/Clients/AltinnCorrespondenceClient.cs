using Arbeidstilsynet.Common.Altinn.Correspondence;
using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Mapping;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.AspNetCore.Http;
using Generated = Arbeidstilsynet.Common.Altinn.Correspondence.Models;

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

    public async Task<CorrespondenceLookupResponse> GetCorrespondences(
        string? resourceId = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CorrespondenceStatus? status = null,
        CorrespondencesRoleType? role = null,
        string? onBehalfOf = null,
        string? sendersReference = null,
        Guid? idempotentKey = null,
        int? altinn2CorrespondenceId = null,
        CancellationToken cancellationToken = default
    )
    {
        var response = await client.Correspondence.Api.V1.Correspondence.GetAsync(
            configuration =>
            {
                configuration.QueryParameters.ResourceId = resourceId;
                configuration.QueryParameters.From = from;
                configuration.QueryParameters.To = to;
                configuration.QueryParameters.Status = ParseEnum<Generated.CorrespondenceStatusExt>(
                    status
                );
                configuration.QueryParameters.Role = ParseEnum<Generated.CorrespondencesRoleType>(
                    role
                );
                configuration.QueryParameters.OnBehalfOf = onBehalfOf;
                configuration.QueryParameters.SendersReference = sendersReference;
                configuration.QueryParameters.IdempotentKey = idempotentKey;
                configuration.QueryParameters.Altinn2CorrespondenceId = altinn2CorrespondenceId;
            },
            cancellationToken
        );

        return response?.ToLookupResponse()
            ?? throw new InvalidOperationException("Failed to retrieve correspondences");
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

    private static TTarget? ParseEnum<TTarget>(Enum? value)
        where TTarget : struct, Enum =>
        Enum.TryParse<TTarget>(value?.ToString(), out var parsed) ? parsed : null;
}
