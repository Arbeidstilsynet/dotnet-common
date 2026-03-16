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
}
