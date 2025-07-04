using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;
using Arbeidstilsynet.Common.GeoNorge.DependencyInjection;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Model.Response;
using Arbeidstilsynet.Common.GeoNorge.Ports;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal partial class FylkeKommuneClient : IFylkeKommuneApi
{
    private readonly HttpClient _httpClient;
    
    public FylkeKommuneClient(
        IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(DependencyInjectionExtensions.GeoNorgeClientKey);
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
        var regex = FylkesnummerRegex();
        
        if (!regex.IsMatch(fylkesnummer))
        {
            throw new ArgumentException(
                $"Invalid fylkesnummer format: {fylkesnummer}. Must be 2 digits.",
                nameof(fylkesnummer)
            );
        }
        
        var response = await _httpClient.GetFromJsonAsync<FylkeFullInfoResponse>($"kommuneinfo/v1/fylker/{fylkesnummer}");

        return response?.ToFylkeFullInfo();
    }

    public async Task<KommuneFullInfo?> GetKommuneByNumber(string kommunenummer)
    {
        var regex = KommunenummerRegex();
        
        if (!regex.IsMatch(kommunenummer))
        {
            throw new ArgumentException(
                $"Invalid kommunenummer format: {kommunenummer}. Must be 4 digits.",
                nameof(kommunenummer)
            );
        }
        
        var response = await _httpClient.GetFromJsonAsync<KommuneFullInfoResponse>($"kommuneinfo/v1/kommuner/{kommunenummer}");
        
        return response?.ToKommuneFullInfo();
    }

    public Task<Kommune?> GetKommuneByPoint(PointQuery query)
    {
        var uri = new Uri("kommuneinfo/v1/punkt", UriKind.Relative)
            .AddQueryParameters(query.ToMap());
        
        return _httpClient.GetFromJsonAsync<Kommune>(uri);
    }

    [GeneratedRegex(@"^\d{4}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
    private static partial Regex KommunenummerRegex();
    [GeneratedRegex(@"^\d{2}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
    private static partial Regex FylkesnummerRegex();
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
