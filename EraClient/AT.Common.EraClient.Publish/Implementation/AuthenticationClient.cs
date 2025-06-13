using System.Text.Json;
using Arbeidstilsynet.Common.EraClient;
using Arbeidstilsynet.Common.EraClient.DependencyInjection;
using Arbeidstilsynet.Common.EraClient.Extensions;
using Arbeidstilsynet.Common.EraClient.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Arbeidstilsynet.Common.EraClient;

internal class AuthenticationClient : IAuthenticationClient
{
    private readonly HttpClient _authenticationHttpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly EraClientConfiguration _eraClientConfiguration;

    public AuthenticationClient(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IHostEnvironment hostEnvironment,
        EraClientConfiguration eraClientConfiguration
    )
    {
        _authenticationHttpClient = httpClientFactory.CreateClient(
            DependencyInjection.DependencyInjectionExtensions.AUTHCLIENT_KEY
        );
        _httpContextAccessor = httpContextAccessor;
        _hostEnvironment = hostEnvironment;
        _eraClientConfiguration = eraClientConfiguration;
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
            GetAuthenticationUrl(
                _hostEnvironment.GetRespectiveEraEnvironment(_httpContextAccessor.HttpContext),
                _eraClientConfiguration
            ),
            new FormUrlEncodedContent(dict)
        );
        response.EnsureSuccessStatusCode();
        var contentAsString = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<AuthenticationResponseDto>(contentAsString)
            ?? throw new InvalidOperationException("Deserialization of AuthenticationDto failed.");
    }

    private static string GetAuthenticationUrl(
        Model.EraEnvironment eraEnvironment,
        EraClientConfiguration config
    )
    {
        if (config.AuthenticationUrl != null)
        {
            return config.AuthenticationUrl;
        }
        return eraEnvironment switch
        {
            Model.EraEnvironment.Verifi =>
                "https://dev-altinn-bemanning.auth.eu-west-1.amazoncognito.com/oauth2/token",
            Model.EraEnvironment.Valid =>
                "https://test-altinn-bemanning.auth.eu-west-1.amazoncognito.com/oauth2/token",
            Model.EraEnvironment.Prod =>
                "https://prod-altinn-bemanning.auth.eu-west-1.amazoncognito.com/oauth2/token",
            _ => throw new InvalidOperationException(),
        };
    }
}
