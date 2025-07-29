using Altinn.App.Core.Models;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.Altinn.Implementation;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

public static class AltinnStorageClientExtensions
{
    public static async Task<IEnumerable<Instance>> GetAllInstances(
        this IAltinnStorageClient altinnStorageClient,
        InstanceQueryParameters queryParameters
    )
    {
        var visitedUris = new HashSet<string>();
        
        var queryResponse = await altinnStorageClient.GetInstances(queryParameters);

        var instances = new List<Instance>(queryResponse.Instances);
        
        while (Uri.IsWellFormedUriString(queryResponse.Next, UriKind.Absolute) 
               && visitedUris.Add(queryResponse.Next)
               && queryParameters.TryAppendContinuationToken(new Uri(queryResponse.Next), out queryParameters))
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