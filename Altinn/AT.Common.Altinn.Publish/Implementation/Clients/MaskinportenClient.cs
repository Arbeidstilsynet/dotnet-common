using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.App.Core.Infrastructure.Clients.Events;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Model.Api;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using static Arbeidstilsynet.Common.Altinn.DependencyInjection.DependencyInjectionExtensions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class MaskinportenClient : IMaskinportenClient
{
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
        var jwtGrant = _config.Value.GenerateJwtGrant(_httpClient.BaseAddress!);

        var dict = new Dictionary<string, string>
        {
            { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
            { "assertion", jwtGrant },
        };
        return await _httpClient
                .Post("token", new FormUrlEncodedContent(dict))
                .ReceiveContent<MaskinportenTokenResponse>(_jsonSerializerOptions)
            ?? throw new Exception("Failed to subscribe to Altinn");
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
