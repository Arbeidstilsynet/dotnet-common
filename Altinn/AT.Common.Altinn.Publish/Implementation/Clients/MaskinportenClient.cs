using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.App.Core.Infrastructure.Clients.Events;
using Arbeidstilsynet.Common.Altinn.Model.Api;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Newtonsoft.Json;
using static Arbeidstilsynet.Common.Altinn.DependencyInjection.DependencyInjectionExtensions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class MaskinportenClient : IMaskinportenClient
{
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public MaskinportenClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(MaskinportenApiClientKey);
        _jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public Uri BaseUrl()
    {
        return _httpClient.BaseAddress
            ?? throw new InvalidOperationException("HttpClient does not have a base address set.");
    }

    public async Task<MaskinportenTokenResponse> GetToken(string jwtGrant)
    {
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
