using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

internal class AltinnMeldingerAdapter(IAltinnCorrespondenceClient correspondenceClient)
    : IAltinnMeldingerAdapter
{
    public Task<CorrespondenceResponse> CreateCorrespondence(
        CorrespondenceRequest request,
        List<IFormFile>? attachments = null
    )
    {
        return correspondenceClient.InitializeCorrespondence(request.ToApiRequest(), attachments);
    }

    public async Task<AltinnCorrespondenceOverview?> GetCorrespondence(Guid correspondenceId)
    {
        try
        {
            return await correspondenceClient.GetCorrespondence(correspondenceId);
        }
        catch (AltinnHttpRequestException e)
        {
            if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            throw;
        }
    }
}
