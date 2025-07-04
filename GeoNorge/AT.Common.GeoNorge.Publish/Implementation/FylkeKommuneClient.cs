using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Arbeidstilsynet.Common.GeoNorge.DependencyInjection;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Model.Response;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal class FylkeKommuneClient : IFylkeKommuneApi
{
    private readonly ILogger<FylkeKommuneClient> _logger;
    private readonly HttpClient _httpClient;
    
    public FylkeKommuneClient(
        IHttpClientFactory httpClientFactory,
        ILogger<FylkeKommuneClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient(DependencyInjectionExtensions.GeoNorgeClientKey);
        _logger = logger;
    }


    public async Task<IEnumerable<Fylke>> GetFylker()
    {
        return await _httpClient.GetFromJsonAsync<List<Fylke>>("kommuneinfo/v1/fylker") ?? [];
    }

    public async Task<IEnumerable<Kommune>> GetKommuner()
    {
        return await _httpClient.GetFromJsonAsync<List<Kommune>>("kommuneinfo/v1/kommuner") ?? [];
    }

    public async Task<IEnumerable<FylkeFullInfo>> GetFylkerFullInfo()
    {
        var response = await _httpClient.GetFromJsonAsync<List<FylkeFullInfoResponse>>(
            "kommuneinfo/v1/fylkerkommuner"
        );
        
        return response?.Select(f => f.ToFylkeFullInfo()) ?? [];
    }

    public async Task<FylkeFullInfo?> GetFylkeByNumber(string fylkesnummer)
    {
        var response = await _httpClient.GetFromJsonAsync<FylkeFullInfoResponse>($"kommuneinfo/v1/fylker/{fylkesnummer}");

        return response?.ToFylkeFullInfo();
    }

    public async Task<KommuneFullInfo?> GetKommuneByNumber(string kommunenummer)
    {
        var response = await _httpClient.GetFromJsonAsync<KommuneFullInfoResponse>($"kommuneinfo/v1/kommuner/{kommunenummer}");
        
        return response?.ToKommuneFullInfo();
    }

    public Task<Kommune?> GetKommuneByPoint(PointQuery query)
    {
        throw new NotImplementedException();
    }
}

file record FylkeFullInfoResponse
{
    [JsonPropertyName("fylkesnummer")]
    public string Fylkesnummer { get; init; } = string.Empty;
    [JsonPropertyName("fylkesnavn")]
    public string Fylkesnavn { get; init; } = string.Empty;
    [JsonPropertyName("kommuner")]
    public List<KommuneFullInfoResponse> Kommuner { get; init; } = [];
}


file record KommuneFullInfoResponse
{
    [JsonPropertyName("fylkesnummer")]
    public string Fylkesnummer { get; init; } = string.Empty;
    
    [JsonPropertyName("kommunenummer")]
    public string Kommunenummer { get; init; } = string.Empty;
    [JsonPropertyName("kommunenavn")]
    public string Kommunenavn { get; init; } = string.Empty;
    
    [JsonPropertyName("punktIOmrade")]
    public GeoJson PunktIOmråde { get; init; } = new GeoJson();
}

file record GeoJson
{
    [JsonPropertyName("coordinates")]
    public List<double> Coordinates { get; init; } = [];
}

file static class MappingExtensions
{
    public static FylkeFullInfo ToFylkeFullInfo(
        this FylkeFullInfoResponse response
    )
    {
        return new FylkeFullInfo
        {
            Fylke = new Fylke
            {
                Fylkesnummer = response.Fylkesnummer,
                Fylkesnavn = response.Fylkesnavn
            },
            Kommuner = response.Kommuner.Select(k => k.ToKommuneFullInfo()).ToList()
        };
    }
    
    public static KommuneFullInfo ToKommuneFullInfo(
        this KommuneFullInfoResponse response
    )
    {
        return new KommuneFullInfo
        {
            Fylkesnummer = response.Fylkesnummer,
            Kommune = new Kommune
            {
                Kommunenummer = response.Kommunenummer,
                Kommunenavn = response.Kommunenavn
            },
            Location = new Location
            {
                Longitude = response.PunktIOmråde.Coordinates[0],
                Latitude = response.PunktIOmråde.Coordinates[1]
            }
        };
    }
}
