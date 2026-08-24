using Arbeidstilsynet.Common.Altinn.Dialogporten;
using Arbeidstilsynet.Common.Altinn.Dialogporten.Models;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnDialogportenClient(DialogportenApiClient client) : IAltinnDialogportenClient
{
    public async Task<V1CommonIdentifierLookup_ServiceOwnerIdentifierLookup> LookupDialog(
        string instanceRef,
        CancellationToken cancellationToken = default
    )
    {
        return await client.Api.V1.Serviceowner.Dialoglookup.GetAsync(
                request => request.QueryParameters.InstanceRef = instanceRef,
                cancellationToken
            ) ?? throw new InvalidOperationException("Failed to look up dialog");
    }
}
