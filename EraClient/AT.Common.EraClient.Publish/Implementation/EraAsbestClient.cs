using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Arbeidstilsynet.Common.EraClient;
using Arbeidstilsynet.Common.EraClient.Model;
using Arbeidstilsynet.Common.EraClient.Model.Asbest;

namespace Arbeidstilsynet.Common.EraClient;

internal class EraAsbestClient : IEraAsbestClient
{
    private readonly HttpClient _httpClient;

    public EraAsbestClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(
            DependencyInjection.DependencyInjectionExtensions.ASBESTCLIENT_KEY
        );
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
        return await _httpClient.GetFromJsonAsync<SøknadStatusResponse>(
            new Uri($"soknad/{orgNumber}", UriKind.Relative)
        );
    }
}
