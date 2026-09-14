using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for interacting with Altinn Correspondence (meldinger).
/// </summary>
public interface IAltinnCorrespondenceClient
{
    /// <summary>
    /// Initializes a new correspondence / altinn melding.
    /// </summary>
    /// <param name="request">The correspondence request details.</param>
    /// <param name="attachments">File attachments if any, else null</param>
    /// <returns>The created correspondence mapped to our internal model.</returns>
    Task<CorrespondenceResponse> InitializeCorrespondence(
        InitializeCorrespondences request,
        List<IFormFile>? attachments
    );

    /// <summary>
    /// Returns an existing correspondence, throws <see cref="AltinnHttpRequestException"/> if it fails.
    /// </summary>
    /// <param name="guid">The identifier of an existing correspondence</param>
    /// <returns></returns>
    Task<AltinnCorrespondenceOverview> GetCorrespondence(Guid guid);

    /// <summary>
    /// Looks up existing correspondences matching the provided query parameters.
    /// Mirrors the Altinn GET <c>/correspondence</c> endpoint. All parameters are optional.
    /// Throws <see cref="AltinnHttpRequestException"/> if the request fails.
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
        int? altinn2CorrespondenceId = null
    );
}
