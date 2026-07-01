using Arbeidstilsynet.Common.GeoNorge.Adresser;
using Arbeidstilsynet.Common.GeoNorge.Adresser.Models;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal class AddressSearchClient(AdresserClient client, ILogger<AddressSearchClient> logger)
    : IAddressSearch
{
    public async Task<OutputAdresseList?> SearchAddresses(
        TextSearchQuery query,
        Pagination? pagination = default
    )
    {
        pagination ??= new Pagination();

        try
        {
            return await client.Sok.GetAsync(config =>
            {
                var parameters = config.QueryParameters;

                if (query.SearchTerm is { Length: > 0 } searchTerm)
                {
                    parameters.Sok = searchTerm;
                }

                if (query.FuzzySearch)
                {
                    parameters.Fuzzy = true;
                }

                if (query.Adressenavn is { Length: > 0 } adressenavn)
                {
                    parameters.Adressenavn = adressenavn;
                }

                if (query.Poststed is { Length: > 0 } poststed)
                {
                    parameters.Poststed = poststed;
                }

                if (query.Postnummer is { Length: > 0 } postnummer)
                {
                    parameters.Postnummer = postnummer;
                }

                if (query.Kommunenummer is { Length: > 0 } kommunenummer)
                {
                    parameters.Kommunenummer = kommunenummer;
                }

                if (query.Gardsnummer is > 0 and var gardsnummer)
                {
                    parameters.Gardsnummer = gardsnummer;
                }

                if (query.Bruksnummer is > 0 and var bruksnummer)
                {
                    parameters.Bruksnummer = bruksnummer;
                }

                ApplyPagination(pagination, out var side, out var treffPerSide);
                parameters.Side = side;
                parameters.TreffPerSide = treffPerSide;
            });
        }
        catch (Exception e) when (e is HttpRequestException or ApiException)
        {
            logger.LogWarning(e, "Failed to get address location for query: {Query}", query);
        }

        return null;
    }

    public async Task<OutputGeoPointList?> SearchAddressesByPoint(
        PointSearchQuery query,
        Pagination? pagination = default
    )
    {
        if (query.RadiusInMeters <= 0)
        {
            throw new ArgumentException(
                "RadiusInMeters must be greater than 0.",
                nameof(query.RadiusInMeters)
            );
        }

        pagination ??= new Pagination();

        try
        {
            return await client.Punktsok.GetAsync(config =>
            {
                var parameters = config.QueryParameters;

                parameters.Lat = (float)query.Latitude;
                parameters.Lon = (float)query.Longitude;
                parameters.Radius = (int)query.RadiusInMeters;

                ApplyPagination(pagination, out var side, out var treffPerSide);
                parameters.Side = side;
                parameters.TreffPerSide = treffPerSide;
            });
        }
        catch (Exception e) when (e is HttpRequestException or ApiException)
        {
            logger.LogWarning(e, "Failed to get address location for query: {Query}", query);
        }

        return null;
    }

    private static void ApplyPagination(Pagination pagination, out int? side, out int? treffPerSide)
    {
        side = pagination.PageIndex >= 0 ? (int)pagination.PageIndex : null;
        treffPerSide = pagination.PageSize > 0 ? (int)pagination.PageSize : null;
    }
}
