using Arbeidstilsynet.Common.GeoNorge.Eiendom;
using Arbeidstilsynet.Common.GeoNorge.Eiendom.Models;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using GeokodingQueryParameters = Arbeidstilsynet.Common.GeoNorge.Eiendom.Geokoding.GeokodingRequestBuilder.GeokodingRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal class PropertySearchClient(EiendomClient client, ILogger<PropertySearchClient> logger)
    : IPropertySearch
{
    private const int DefaultKoordsys = 4326;

    public async Task<GeoKodingRespons?> SearchProperties(GeokodingQueryParameters queryParameters)
    {
        queryParameters.Utkoordsys ??= DefaultKoordsys;

        try
        {
            return await client.Geokoding.GetAsync(config =>
                config.QueryParameters = queryParameters
            );
        }
        catch (Exception e) when (e is HttpRequestException or ApiException)
        {
            logger.LogWarning(
                e,
                "Failed to search properties for query: {@Query}",
                queryParameters
            );
        }

        return null;
    }
}
