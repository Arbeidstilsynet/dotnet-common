using System.Text.Json;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.Extensions.Options;
using static Arbeidstilsynet.Common.Altinn.DependencyInjection.DependencyInjectionExtensions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class MaskinportenClient : IMaskinportenClient
{
    private const int TokenGrace = 60; // seconds

    private DateTime _tokenExpirationTime = DateTime.MinValue;
    private MaskinportenTokenResponse? _currentToken;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IOptions<MaskinportenConfiguration> _config;

    public MaskinportenClient(
        IHttpClientFactory httpClientFactory,
        IOptions<MaskinportenConfiguration> altinnAuthenticationConfigurationOptions
    )
    {
        _httpClient = httpClientFactory.CreateClient(MaskinportenApiClientKey);
        _jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        _config = altinnAuthenticationConfigurationOptions;
    }

    public async Task<MaskinportenTokenResponse> GetToken()
    {
        await _semaphore.WaitAsync();
        try
        {
            return await GetTokenInternal();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<MaskinportenTokenResponse> GetTokenInternal()
    {
        if (TryGetCachedToken(out var cachedToken))
        {
            return cachedToken!;
        }

        var jwtGrant = _config.Value.GenerateJwtGrant(_httpClient.BaseAddress!);

        var dict = new Dictionary<string, string>
        {
            { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
            { "assertion", jwtGrant },
        };

        var tokenResponse =
            await _httpClient
                .Post("token", new FormUrlEncodedContent(dict))
                .ReceiveContent<MaskinportenTokenResponse>(_jsonSerializerOptions)
            ?? throw new Exception("Failed to retrieve Maskinporten token");

        UpdateCachedToken(tokenResponse);

        return tokenResponse;
    }

    private bool TryGetCachedToken(out MaskinportenTokenResponse? token)
    {
        if (_currentToken is null || DateTime.Now >= _tokenExpirationTime)
        {
            token = null;
            return false;
        }

        token = _currentToken;
        return true;
    }

    private void UpdateCachedToken(MaskinportenTokenResponse tokenResponse)
    {
        _currentToken = tokenResponse;
        _tokenExpirationTime = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn - TokenGrace);
    }
}

file static class Extensions
{
    public static string GenerateJwtGrant(this MaskinportenConfiguration config, Uri baseAddress)
    {
        return JwtExtensions.GenerateJwtGrant(
            baseAddress.ToString(),
            config.CertificatePrivateKey,
            config.CertificateChain,
            config.IntegrationId,
            config.Scopes
        );
    }
}
