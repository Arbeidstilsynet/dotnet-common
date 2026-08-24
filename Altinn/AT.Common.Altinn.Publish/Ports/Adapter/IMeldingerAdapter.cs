using Arbeidstilsynet.Common.Altinn.Correspondence.Models;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.Altinn.Ports.Adapter;

/// <summary>
/// Adapter interface for interacting with Altinn correspondences / altinn melding.
/// </summary>
public interface IAltinnMeldingerAdapter
{
    /// <summary>
    /// Gets a summary of an Altinn correspondence.
    /// </summary>
    /// <returns>The summary of the Altinn correspondence, null if we do not find any. If any other exception occurs, a <see cref="AltinnHttpRequestException"/> is thrown.</returns>
    public Task<CorrespondenceOverviewExt?> GetCorrespondence(Guid correspondenceId);

    /// <summary>
    /// Gets a summary of an Altinn correspondence.
    /// </summary>
    /// <param name="request">A request object with all possible options to create the correspondence.</param>
    /// <param name="attachments">File attachments if any, else null</param>
    /// <returns>The summary of the Altinn correspondence.</returns>
    public Task<InitializeCorrespondencesResponseExt> CreateCorrespondence(
        CorrespondenceRequest request,
        List<IFormFile>? attachments = null
    );
}
