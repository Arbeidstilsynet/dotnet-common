using Arbeidstilsynet.Common.Altinn.Correspondence.Models;
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
    Task<InitializeCorrespondencesResponseExt> InitializeCorrespondence(
        InitializeCorrespondencesExt request,
        List<IFormFile>? attachments = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets an overview of a correspondence.
    /// </summary>
    Task<CorrespondenceOverviewExt> GetCorrespondence(
        Guid correspondenceId,
        CancellationToken cancellationToken = default
    );
}
