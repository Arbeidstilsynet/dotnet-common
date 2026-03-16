using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.Altinn.Ports.Adapter;

/// <summary>
/// Adapter interface for interacting with Altinn correspondences / altinn melding.
/// </summary>
public interface IMeldingerAdapter
{
    /// <summary>
    /// Gets a summary of an Altinn correspondence.
    /// </summary>
    /// <returns>The summary of the Altinn correspondence.</returns>
    public Task<CorrespondenceResponse> GetCorrespondence();

    /// <summary>
    /// Gets a summary of an Altinn correspondence.
    /// </summary>
    /// <param name="request">A request object with all possible options to create the correspondence.</param>
    /// <param name="attachments">File attachments if any, else null</param>
    /// <returns>The summary of the Altinn correspondence.</returns>
    public Task<CorrespondenceResponse> CreateCorrespondence(
        CorrespondenceRequest request,
        List<IFormFile>? attachments = null
    );
}
