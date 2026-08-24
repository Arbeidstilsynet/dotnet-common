using Arbeidstilsynet.Common.Altinn.Correspondence.Models;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.Kiota.Abstractions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

internal class AltinnMeldingerAdapter(IAltinnCorrespondenceClient correspondenceClient)
    : IAltinnMeldingerAdapter
{
    public Task<InitializeCorrespondencesResponseExt> CreateCorrespondence(
        CorrespondenceRequest request,
        List<IFormFile>? attachments = null
    )
    {
        return correspondenceClient.InitializeCorrespondence(request.ToApiRequest(), attachments);
    }

    public async Task<CorrespondenceOverviewExt?> GetCorrespondence(Guid correspondenceId)
    {
        try
        {
            return await correspondenceClient.GetCorrespondence(correspondenceId);
        }
        catch (ApiException e)
            when (e.ResponseStatusCode == (int)System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
