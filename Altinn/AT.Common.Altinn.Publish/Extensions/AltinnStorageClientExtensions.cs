using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

public static class AltinnStorageClientExtensions
{
    public static async Task<IEnumerable<AltinnInstance>> GetAllInstances(
        this IAltinnStorageClient altinnStorageClient,
        InstanceQueryParameters queryParameters
    )
    {
        var visitedUris = new HashSet<string>();

        var queryResponse = await altinnStorageClient.GetInstances(queryParameters);

        var instances = new List<AltinnInstance>(queryResponse.Instances);

        while (
            Uri.IsWellFormedUriString(queryResponse.Next, UriKind.Absolute)
            && visitedUris.Add(queryResponse.Next)
            && queryParameters.TryAppendContinuationToken(
                new Uri(queryResponse.Next),
                out queryParameters
            )
        )
        {
            queryResponse = await altinnStorageClient.GetInstances(queryParameters);

            if (queryResponse?.Instances is null)
            {
                break;
            }

            instances.AddRange(queryResponse.Instances);
        }

        return instances;
    }
}
