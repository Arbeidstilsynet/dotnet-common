using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.Kiota.Abstractions;

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

    [Obsolete(
        "Use IAltinnCorrespondenceClient.GetCorrespondences with explicit resourceId, role and idempotentKey instead. "
            + "This method is only a small simplification and is hard-wired to the dat-meldinger-correspondence resource owned by team Meldinger."
    )]
    public Task<CorrespondenceLookupResponse> GetCorrespondenceByIdempotentKey(Guid idempotentKey)
    {
        return correspondenceClient.GetCorrespondences(
            resourceId: "dat-meldinger-correspondence",
            role: CorrespondencesRoleType.Sender,
            idempotentKey: idempotentKey
        );
    }

    public async Task<AltinnCorrespondenceOverview?> GetCorrespondence(Guid correspondenceId)
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
