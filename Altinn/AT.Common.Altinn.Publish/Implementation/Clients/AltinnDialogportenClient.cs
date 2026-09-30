using Arbeidstilsynet.Common.Altinn.Dialogporten;
using Arbeidstilsynet.Common.Altinn.Implementation.Mapping;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnDialogportenClient(DialogportenApiClient client) : IAltinnDialogportenClient
{
    public async Task<DialogportenLookupResponse> LookupDialog(
        string instanceRef,
        CancellationToken cancellationToken = default
    )
    {
        var lookup = await client.Api.V1.Serviceowner.Dialoglookup.GetAsync(
            request => request.QueryParameters.InstanceRef = instanceRef,
            cancellationToken
        );

        return lookup?.ToLookupResponse()
            ?? throw new InvalidOperationException("Failed to look up dialog");
    }
}
