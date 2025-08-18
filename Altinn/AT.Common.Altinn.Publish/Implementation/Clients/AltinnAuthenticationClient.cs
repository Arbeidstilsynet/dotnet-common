using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Altinn.App.Core.Infrastructure.Clients.Events;
using Arbeidstilsynet.Common.Altinn.Model.Api;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using static Arbeidstilsynet.Common.Altinn.DependencyInjection.DependencyInjectionExtensions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnAuthenticationClient : IAltinnAuthenticationClient
{
    private readonly HttpClient _httpClient;

    public AltinnAuthenticationClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(AltinnAuthenticationApiClientKey);
    }

    public async Task<string> ExchangeToken(
        string tokenProviderToken,
        AuthenticationTokenProvider tokenProvider = AuthenticationTokenProvider.Maskinporten
    )
    {
        return await _httpClient
                .Get($"exchange/{tokenProvider.ToString().ToLower()}")
                .WithBearerToken(tokenProviderToken)
                .WithAcceptHeader("text/plain", 1.0)
                .ReceiveString() ?? throw new Exception("Failed to subscribe to Altinn");
    }
}
