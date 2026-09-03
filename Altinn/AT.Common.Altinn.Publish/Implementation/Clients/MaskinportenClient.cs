using System.Collections.Concurrent;
using System.Net.Http.Json;
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

    private readonly ConcurrentDictionary<string, CachedToken> _tokens = new();
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

    public async Task<MaskinportenTokenResponse> GetToken(
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default
    )
    {
        // Scopes are baked into the grant, so a token is only reusable by callers asking for the
        // same set. Order is normalised so equivalent sets share a cache entry.
        var cacheKey = string.Join(' ', scopes.OrderBy(scope => scope, StringComparer.Ordinal));

        if (TryGetCachedToken(cacheKey, out var alreadyCached))
        {
            return alreadyCached!;
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await GetTokenInternal(cacheKey, scopes, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<MaskinportenTokenResponse> GetTokenInternal(
        string cacheKey,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken
    )
    {
        if (TryGetCachedToken(cacheKey, out var cachedToken))
        {
            return cachedToken!;
        }

        var jwtGrant = _config.Value.GenerateJwtGrant(_httpClient.BaseAddress!, scopes);

        var form = new Dictionary<string, string>
        {
            { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
            { "assertion", jwtGrant },
        };

        using var response = await _httpClient.PostAsync(
            "token",
            new FormUrlEncodedContent(form),
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        var tokenResponse =
            await response.Content.ReadFromJsonAsync<MaskinportenTokenResponse>(
                _jsonSerializerOptions,
                cancellationToken
            ) ?? throw new InvalidOperationException("Failed to retrieve Maskinporten token");

        _tokens[cacheKey] = new CachedToken(
            tokenResponse,
            DateTime.Now.AddSeconds(tokenResponse.ExpiresIn - TokenGrace)
        );

        return tokenResponse;
    }

    private bool TryGetCachedToken(string cacheKey, out MaskinportenTokenResponse? token)
    {
        if (_tokens.TryGetValue(cacheKey, out var cached) && DateTime.Now < cached.ExpiresAt)
        {
            token = cached.Token;
            return true;
        }

        token = null;
        return false;
    }

    private sealed record CachedToken(MaskinportenTokenResponse Token, DateTime ExpiresAt);
}

file static class Extensions
{
    public static string GenerateJwtGrant(
        this MaskinportenConfiguration config,
        Uri baseAddress,
        IReadOnlyList<string> scopes
    )
    {
        var audience = baseAddress.ToString();
        var requestedScopes = scopes.ToArray();

        if (config.KeyId is not null)
        {
            return JwtExtensions.GenerateJwtGrantWithKey(
                audience,
                config.PrivateKey,
                config.KeyId,
                config.IntegrationId,
                requestedScopes
            );
        }

        return JwtExtensions.GenerateJwtGrantWithCertificateChain(
            audience,
            config.PrivateKey,
            config.CertificateChain
                ?? throw new InvalidOperationException(
                    "Either KeyId or CertificateChain must be configured."
                ),
            config.IntegrationId,
            requestedScopes
        );
    }
}
