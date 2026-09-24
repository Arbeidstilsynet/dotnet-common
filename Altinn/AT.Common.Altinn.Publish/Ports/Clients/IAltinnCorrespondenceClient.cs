using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for the Altinn correspondence API.
/// </summary>
public interface IAltinnCorrespondenceClient
{
    /// <summary>
    /// Initialises a correspondence, optionally uploading its attachments in the same request.
    /// </summary>
    /// <param name="request">The correspondence to initialise.</param>
    /// <param name="attachments">
    /// Files to upload alongside the correspondence. When supplied, the multipart upload endpoint
    /// is used instead of the JSON one.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<CorrespondenceResponse> InitializeCorrespondence(
        InitializeCorrespondences request,
        List<IFormFile>? attachments = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets an overview of a correspondence.
    /// </summary>
    /// <param name="correspondenceId">The identifier of an existing correspondence.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<AltinnCorrespondenceOverview> GetCorrespondence(
        Guid correspondenceId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Looks up existing correspondences matching the provided query parameters.
    /// Mirrors the Altinn GET <c>/correspondence</c> endpoint. All parameters are optional.
    /// </summary>
    /// <param name="resourceId">The resource identifier the correspondences belong to.</param>
    /// <param name="from">Only include correspondences created on or after this point in time.</param>
    /// <param name="to">Only include correspondences created on or before this point in time.</param>
    /// <param name="status">Only include correspondences with this status.</param>
    /// <param name="role">The role the authenticated party has for the correspondences.</param>
    /// <param name="onBehalfOf">Look up correspondences on behalf of the given party.</param>
    /// <param name="sendersReference">Filter by the senders reference used when initializing the correspondence.</param>
    /// <param name="idempotentKey">The idempotent key used when initializing the correspondence.</param>
    /// <param name="altinn2CorrespondenceId">Filter by a legacy Altinn 2 correspondence id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The ids of the matching correspondences.</returns>
    Task<CorrespondenceLookupResponse> GetCorrespondences(
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
    );
}
