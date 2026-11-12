using System.Runtime.ExceptionServices;
using System.Text.Json;
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
    private const int DefaultKoordsys = 4326;

    public async Task<OutputAdresseList?> SearchAddresses(SokQueryParameters queryParameters)
    {
        queryParameters.Utkoordsys ??= DefaultKoordsys;

        try
        {
            return await client.Sok.GetAsync(config => config.QueryParameters = queryParameters);
        }
        catch (ApiException e) when (e.InnerException is JsonException parseFailure)
        {
            // Kiota wraps parse failures in ApiException; a malformed body must not look like "no results".
            ExceptionDispatchInfo.Throw(parseFailure);
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
        queryParameters.Koordsys ??= DefaultKoordsys;
        queryParameters.Utkoordsys ??= DefaultKoordsys;

        try
        {
            return await client.Punktsok.GetAsync(config =>
                config.QueryParameters = queryParameters
            );
        }
        catch (ApiException e) when (e.InnerException is JsonException parseFailure)
        {
            // Kiota wraps parse failures in ApiException; a malformed body must not look like "no results".
            ExceptionDispatchInfo.Throw(parseFailure);
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
