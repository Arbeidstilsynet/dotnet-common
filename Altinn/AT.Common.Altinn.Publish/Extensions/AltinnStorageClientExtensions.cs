using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Storage.Models;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

/// <summary>
/// Extension methods for IAltinnStorageClient
/// </summary>
public static class AltinnStorageClientExtensions
{
    /// <summary>
    /// Retrieves all Altinn instances matching the given query, handling pagination internally.
    /// </summary>
    /// <param name="altinnStorageClient">The storage client.</param>
    /// <param name="query">The instance query.</param>
    public static async Task<IEnumerable<Instance>> GetAllInstances(
        this IAltinnStorageClient altinnStorageClient,
        InstanceQuery query
    )
    {
        var visitedUris = new HashSet<string>();

        var queryResponse = await altinnStorageClient.GetInstances(query);

        var instances = new List<Instance>(queryResponse.Instances ?? []);

        while (
            Uri.IsWellFormedUriString(queryResponse.Next, UriKind.Absolute)
            && visitedUris.Add(queryResponse.Next)
            && query.TryAppendContinuationToken(new Uri(queryResponse.Next), out query)
        )
        {
            queryResponse = await altinnStorageClient.GetInstances(query);

            if (queryResponse?.Instances is null)
            {
                break;
            }

            instances.AddRange(queryResponse.Instances);
        }

        return instances;
    }
}
