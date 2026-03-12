using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for subscribing to Altinn events.
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
        CorrespondenceRequest request,
        List<IFormFile>? attachments
    );
}
