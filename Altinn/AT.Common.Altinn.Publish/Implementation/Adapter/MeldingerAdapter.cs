using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

internal class MeldingerAdapter(IAltinnCorrespondenceClient correspondenceClient)
    : IMeldingerAdapter
{
    public Task<CorrespondenceResponse> CreateCorrespondence(
        CorrespondenceRequest request,
        List<IFormFile>? attachments = null
    )
    {
        return correspondenceClient.InitializeCorrespondence(request.ToApiRequest(), attachments);
    }

    public Task<CorrespondenceResponse> GetCorrespondence()
    {
        throw new NotImplementedException();
    }
}
