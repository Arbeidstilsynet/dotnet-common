using System.Net.Http.Json;
using System.Text.Json;
using Arbeidstilsynet.Common.EraClient.Ports;
using Arbeidstilsynet.Common.EraClient.Ports.Model;
using Arbeidstilsynet.Common.EraClient.Ports.Model.Asbest;

namespace Arbeidstilsynet.Common.EraClient.Adapters;

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
        _httpClient.DefaultRequestHeaders.Add(
            "Authorization",
            $"Bearer {authenticationResponse.AccessToken}"
        );
        return await _httpClient.GetFromJsonAsync<List<Ports.Model.Asbest.Melding>>(
                new Uri(orgNumber + "/meldinger", UriKind.Relative)
            ) ?? [];
    }
}
