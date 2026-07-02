using Arbeidstilsynet.Common.GeoNorge.Adresser;
using Arbeidstilsynet.Common.GeoNorge.Adresser.Models;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using PunktsokQueryParameters = Arbeidstilsynet.Common.GeoNorge.Adresser.Punktsok.PunktsokRequestBuilder.PunktsokRequestBuilderGetQueryParameters;
using SokQueryParameters = Arbeidstilsynet.Common.GeoNorge.Adresser.Sok.SokRequestBuilder.SokRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal class AddressSearchClient(AdresserClient client, ILogger<AddressSearchClient> logger)
    : IAddressSearch
{
    public async Task<OutputAdresseList?> SearchAddresses(SokQueryParameters queryParameters)
    {
        try
        {
            return await client.Sok.GetAsync(config => config.QueryParameters = queryParameters);
        }
        catch (Exception e) when (e is HttpRequestException or ApiException)
        {
            logger.LogWarning(e, "Failed to search addresses for query: {@Query}", queryParameters);
        }

        return null;
    }

    public async Task<OutputGeoPointList?> SearchAddressesByPoint(
        PunktsokQueryParameters queryParameters
    )
    {
        try
        {
            return await client.Punktsok.GetAsync(config =>
                config.QueryParameters = queryParameters
            );
        }
        catch (Exception e) when (e is HttpRequestException or ApiException)
        {
            logger.LogWarning(
                e,
                "Failed to search addresses by point for query: {@Query}",
                queryParameters
            );
        }

        return null;
    }
}
