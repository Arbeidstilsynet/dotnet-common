using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Arbeidstilsynet.Common.EraClient;
using Arbeidstilsynet.Common.EraClient.DependencyInjection;
using Arbeidstilsynet.Common.EraClient.Extensions;
using Arbeidstilsynet.Common.EraClient.Model;
using Arbeidstilsynet.Common.EraClient.Model.Asbest;
using Microsoft.Extensions.Hosting;

namespace Arbeidstilsynet.Common.EraClient;

internal class EraAsbestClient : IEraAsbestClient
{
    private readonly HttpClient _httpClient;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly EraClientConfiguration _eraClientConfiguration;

    public EraAsbestClient(
        IHttpClientFactory httpClientFactory,
        IHostEnvironment hostEnvironment,
        EraClientConfiguration eraClientConfiguration
    )
    {
        _httpClient = httpClientFactory.CreateClient(
            DependencyInjection.DependencyInjectionExtensions.ASBESTCLIENT_KEY
        );
        _hostEnvironment = hostEnvironment;
        _eraClientConfiguration = eraClientConfiguration;
    }

    public async Task<List<Melding>> GetMeldingerByOrg(
        AuthenticationResponseDto authenticationResponse,
        string orgNumber
    )
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            authenticationResponse.TokenType,
            authenticationResponse.AccessToken
        );
        // we need to update the base adress before each request because the determination of the base url is not defined at compile time
        _httpClient.BaseAddress = new Uri(GetAsbestUrl(_hostEnvironment, _eraClientConfiguration));
        return await _httpClient.GetFromJsonAsync<List<Model.Asbest.Melding>>(
                new Uri($"{orgNumber}/meldinger", UriKind.Relative)
            ) ?? [];
    }

    public async Task<SøknadStatusResponse?> GetStatusForExistingSøknad(
        AuthenticationResponseDto authenticationResponse,
        string orgNumber
    )
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            authenticationResponse.TokenType,
            authenticationResponse.AccessToken
        );
        // we need to update the base adress before each request because the determination of the base url is not defined at compile time
        _httpClient.BaseAddress = new Uri(GetAsbestUrl(_hostEnvironment, _eraClientConfiguration));
        return await _httpClient.GetFromJsonAsync<SøknadStatusResponse>(
            new Uri($"soknad/{orgNumber}", UriKind.Relative)
        );
    }

    private static string GetAsbestUrl(
        IHostEnvironment hostEnvironment,
        EraClientConfiguration config
    )
    {
        if (config.EraAsbestUrl != null)
        {
            return config.EraAsbestUrl;
        }
        return hostEnvironment.GetRespectiveEraEnvironment() switch
        {
            Model.EraEnvironment.Verifi =>
                "https://data-verifi.arbeidstilsynet.no/asbest/api/virksomheter/",
            Model.EraEnvironment.Valid =>
                "https://data-valid.arbeidstilsynet.no/asbest/api/virksomheter/",
            Model.EraEnvironment.Prod => "https://data.arbeidstilsynet.no/asbest/api/virksomheter/",
            _ => throw new InvalidOperationException(),
        };
    }
}
