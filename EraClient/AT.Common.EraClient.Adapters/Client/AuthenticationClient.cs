using System.Text.Json;
using Arbeidstilsynet.Common.EraClient.Ports;
using Arbeidstilsynet.Common.EraClient.Ports.Model;

namespace Arbeidstilsynet.Common.EraClient.Adapters;

internal class AuthenticationClient : IAuthenticationClient
{
    private readonly HttpClient _authenticationHttpClient;

    public AuthenticationClient(IHttpClientFactory httpClientFactory)
    {
        _authenticationHttpClient = httpClientFactory.CreateClient(
            DependencyInjection.DependencyInjectionExtensions.AUTHCLIENT_KEY
        );
    }

    public async Task<AuthenticationResponseDto> Authenticate(
        AuthenticationRequestDto authenticationRequestDto
    )
    {
        var dict = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", authenticationRequestDto.ClientId },
            { "client_secret", authenticationRequestDto.ClientSecret },
        };

        var response = await _authenticationHttpClient.PostAsync(
            string.Empty,
            new FormUrlEncodedContent(dict)
        );
        response.EnsureSuccessStatusCode();
        var contentAsString = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<AuthenticationResponseDto>(contentAsString)
            ?? throw new InvalidOperationException("Deserialization of AuthenticationDto failed.");
    }
}
