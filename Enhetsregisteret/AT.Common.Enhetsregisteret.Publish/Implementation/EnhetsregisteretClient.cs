using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;
using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Response;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Implementation;

internal class EnhetsregisteretClient : IEnhetsregisteret
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new EndringstypeJsonConverter() },
    };
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<EnhetsregisteretClient> _logger;
    private readonly CacheOptions _cacheOptions;

    private readonly HttpClient? _optionalClient;

    private HttpClient Client =>
        _optionalClient
        ?? _httpClientFactory?.CreateClient(DependencyInjectionExtensions.Clientkey)
        ?? throw new InvalidOperationException(
            "The http client must either be provided directly or via an IHttpClientFactory."
        );

    public EnhetsregisteretClient(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        EnhetsregisteretConfig config,
        ILogger<EnhetsregisteretClient> logger
    )
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _logger = logger;
        _cacheOptions = config.CacheOptions;
    }

    public EnhetsregisteretClient(EnhetsregisteretConfig config)
    {
        _logger = new NullLogger<EnhetsregisteretClient>();
        _optionalClient = new HttpClient()
        {
            BaseAddress = new Uri(config.BrregApiBaseUrlOverwrite),
        };
        _memoryCache = new MemoryCache(optionsAccessor: new MemoryCacheOptions { });
        _cacheOptions = config.CacheOptions;
    }

    public async Task<Underenhet?> GetUnderenhet(string organisasjonsnummer)
    {
        organisasjonsnummer.ValidateOrgnummerOrThrow(nameof(organisasjonsnummer));

        var uri = new Uri(
            $"enhetsregisteret/api/underenheter/{organisasjonsnummer}",
            UriKind.Relative
        );

        return await GetOrCache<Underenhet>(uri);
    }

    public async Task<Enhet?> GetEnhet(string organisasjonsnummer)
    {
        organisasjonsnummer.ValidateOrgnummerOrThrow(nameof(organisasjonsnummer));

        var uri = new Uri($"enhetsregisteret/api/enheter/{organisasjonsnummer}", UriKind.Relative);

        return await GetOrCache<Enhet>(uri);
    }

    public async Task<PaginationResult<Underenhet>?> SearchUnderenheter(
        SearchEnheterQuery searchParameters,
        Pagination pagination
    )
    {
        var uri = new Uri("enhetsregisteret/api/underenheter", UriKind.Relative)
            .AddQueryParameters(searchParameters.ToMap())
            .AddQueryParameters(pagination.ToMap());

        var response = await GetOrCache<UnderenheterResponse>(uri);

        return response?.Page.ToPaginationResult(response.Embedded?.Underenheter ?? []);
    }

    public async Task<PaginationResult<Enhet>?> SearchEnheter(
        SearchEnheterQuery searchParameters,
        Pagination pagination
    )
    {
        var uri = new Uri("enhetsregisteret/api/enheter", UriKind.Relative)
            .AddQueryParameters(searchParameters.ToMap())
            .AddQueryParameters(pagination.ToMap());

        var response = await GetOrCache<EnheterResponse>(uri);

        return response?.Page.ToPaginationResult(response.Embedded?.Enheter ?? []);
    }

    public async Task<PaginationResult<Oppdatering>?> GetOppdateringerUnderenheter(
        GetOppdateringerQuery query,
        Pagination pagination
    )
    {
        var uri = new Uri("enhetsregisteret/api/oppdateringer/underenheter", UriKind.Relative)
            .AddQueryParameters(query.ToMap())
            .AddQueryParameters(pagination.ToMap());

        var response = await GetOrCache<OppdateringerUnderenheterResponse>(uri);

        return response?.Page?.ToPaginationResult(response.Embedded?.OppdaterteUnderenheter ?? []);
    }

    public async Task<PaginationResult<Oppdatering>?> GetOppdateringerEnheter(
        GetOppdateringerQuery query,
        Pagination pagination
    )
    {
        var uri = new Uri("enhetsregisteret/api/oppdateringer/enheter", UriKind.Relative)
            .AddQueryParameters(query.ToMap())
            .AddQueryParameters(pagination.ToMap());

        var response = await GetOrCache<OppdateringerEnheterResponse>(uri);

        return response?.Page?.ToPaginationResult(response.Embedded?.OppdaterteEnheter ?? []);
    }

    private async Task<T?> GetOrCache<T>(Uri requestUri)
        where T : class
    {
        var cacheKey = requestUri.ToString();

        if (
            !_cacheOptions.Disabled
            && _memoryCache.TryGetValue(cacheKey, out T? cacheHit)
            && cacheHit != null
        )
        {
            return cacheHit;
        }

        try
        {
            var response = await Client.GetFromJsonAsync<T>(requestUri, JsonSerializerOptions);

            if (!_cacheOptions.Disabled)
            {
                _memoryCache.Set(cacheKey, response);
            }
            return response;
        }
        catch (HttpRequestException e)
        {
            if (e is { StatusCode: >= HttpStatusCode.InternalServerError })
            {
                throw;
            }

            _logger.LogWarning(
                e,
                "Request for resource failed({StatusCode}): {Query}",
                e.StatusCode,
                cacheKey
            );
            return null;
        }
    }
}

internal record OppdateringerEnheterResponse
    : EmbeddedResponse<OppdateringEnheterEmbeddedWrapper> { }

internal record OppdateringerUnderenheterResponse
    : EmbeddedResponse<OppdateringUnderenheterEmbeddedWrapper> { }

internal record EnheterResponse : EmbeddedResponse<EnhetEmbeddedWrapper> { }

internal record UnderenheterResponse : EmbeddedResponse<UnderenhetEmbeddedWrapper> { }

internal record EmbeddedResponse<T>
{
    [JsonPropertyName("_embedded")]
    public T? Embedded { get; init; }

    [JsonPropertyName("page")]
    public Page? Page { get; init; }
}

internal record EnhetEmbeddedWrapper
{
    [JsonPropertyName("enheter")]
    public List<Enhet>? Enheter { get; init; }
}

internal record UnderenhetEmbeddedWrapper
{
    [JsonPropertyName("underenheter")]
    public List<Underenhet>? Underenheter { get; init; }
}

internal record OppdateringUnderenheterEmbeddedWrapper
{
    [JsonPropertyName("oppdaterteUnderenheter")]
    public List<Oppdatering>? OppdaterteUnderenheter { get; init; }
}

internal record OppdateringEnheterEmbeddedWrapper
{
    [JsonPropertyName("oppdaterteEnheter")]
    public List<Oppdatering>? OppdaterteEnheter { get; init; }
}

internal record Page
{
    [JsonPropertyName("size")]
    public int Size { get; init; }

    [JsonPropertyName("totalElements")]
    public int TotalElements { get; init; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }

    [JsonPropertyName("number")]
    public int Number { get; init; }
}

file static class Extensions
{
    public static PaginationResult<TElements> ToPaginationResult<TElements>(
        this Page? page,
        IEnumerable<TElements> elements
    )
    {
        if (page == null)
        {
            return EmptyPagination(elements);
        }

        return new PaginationResult<TElements>
        {
            Elements = elements,
            TotalElements = page.TotalElements,
            PageIndex = page.Number,
            PageSize = page.Size,
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
