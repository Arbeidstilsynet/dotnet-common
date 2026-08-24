using Arbeidstilsynet.Common.Altinn.Apps;
using Arbeidstilsynet.Common.Altinn.Apps.Models;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnAppsClient(AppsApiClient client, IOptions<AltinnConfiguration> configuration)
    : IAltinnAppsClient
{
    public async Task<Instance> CompleteInstance(
        string appId,
        InstanceRequest instanceAddress,
        CancellationToken cancellationToken = default
    )
    {
        return await client[configuration.Value.OrgId][appId]
                .Instances[instanceAddress.GetInstanceOwnerPartyId()][instanceAddress.InstanceGuid]
                .Complete.PostAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to complete instance");
    }
}
