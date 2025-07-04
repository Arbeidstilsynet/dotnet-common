using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;
using Arbeidstilsynet.Common.GeoNorge.DependencyInjection;
using Arbeidstilsynet.Common.GeoNorge.Model;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Model.Response;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Microsoft.Extensions.Logging;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal class AddressSearchClient : IAddressSearch
{
    private readonly ILogger<AddressSearchClient> _logger;
    private readonly HttpClient _httpClient;

    public AddressSearchClient(IHttpClientFactory httpClientFactory, ILogger<AddressSearchClient> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(DependencyInjectionExtensions.GeoNorgeClientKey);
    }

    public async Task<PaginationResult<Address>?> SearchAddresses(
        TextSearchQuery query,
        Pagination? pagination = default
    )
    {
        pagination ??= new Pagination();

        var parameterizedUri = new Uri("adresser/v1/sok", UriKind.Relative)
            .AddQueryParameters(query.ToMap())
            .AddQueryParameters(pagination.ToMap());

        try
        {
            var response = await _httpClient.GetFromJsonAsync<SearchResponse>(parameterizedUri);

            return response?.Metadata.ToPaginationResult(response.Addresses);
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "Failed to get address location for query: {Query}", query);
        }

        return null;
    }

    public async Task<PaginationResult<Address>?> SearchAddressesByPoint(
        PointSearchQuery query,
        Pagination? pagination = default
    )
    {
        if (query.RadiusInMeters == 0)
        {
            throw new ArgumentException(
                "RadiusInMeters must be greater than 0.",
                nameof(query.RadiusInMeters)
            );
        }

        pagination ??= new Pagination();

        var parameterizedUri = new Uri("adresser/v1/punktsok", UriKind.Relative)
            .AddQueryParameters(query.ToMap())
            .AddQueryParameters(pagination.ToMap());

        try
        {
            var response = await _httpClient.GetFromJsonAsync<SearchResponse>(parameterizedUri);

            return response?.Metadata.ToPaginationResult(response.Addresses);
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "Failed to get address location for query: {Query}", query);
        }

        return null;
    }
}

internal record SearchResponse
{
    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; set; } = new();

    [JsonPropertyName("adresser")]
    public List<Address> Addresses { get; set; } = [];
}

internal record Metadata
{
    [JsonPropertyName("treffPerSide")]
    public int TreffPerSide { get; set; }

    [JsonPropertyName("totaltAntallTreff")]
    public long TotaltAntallTreff { get; set; }

    [JsonPropertyName("side")]
    public int Side { get; set; }
}

file static class Extensions
{
    public static PaginationResult<TElements> ToPaginationResult<TElements>(
        this Metadata? metadata,
        IEnumerable<TElements> elements
    )
    {
        if (metadata == null)
        {
            return EmptyPagination(elements);
        }

        return new PaginationResult<TElements>
        {
            Elements = elements,
            TotalElements = metadata.TotaltAntallTreff,
            PageIndex = metadata.Side,
            PageSize = metadata.TreffPerSide,
        };
    }

    private static PaginationResult<TElements> EmptyPagination<TElements>(
        IEnumerable<TElements>? elements = null
    )
    {
        return new PaginationResult<TElements>
        {
            Elements = elements ?? [],
            TotalElements = 0,
            PageIndex = 0,
            PageSize = 0,
        };
    }
}
