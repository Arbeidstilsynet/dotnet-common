using Arbeidstilsynet.Common.Altinn.Apps;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Mapping;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnAppsClient(AppsApiClient client, IOptions<AltinnConfiguration> configuration)
    : IAltinnAppsClient
{
    public async Task<AltinnInstance> CompleteInstance(
        string appId,
        InstanceRequest instanceAddress,
        CancellationToken cancellationToken = default
    )
    {
        var completed = await client[configuration.Value.OrgId]
            [appId]
            .Instances[instanceAddress.GetInstanceOwnerPartyId()][instanceAddress.InstanceGuid]
            .Complete.PostAsync(cancellationToken: cancellationToken);

        return completed?.ToAltinnInstance()
            ?? throw new InvalidOperationException("Failed to complete instance");
    }
}
