using System.Text.Json;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using static Arbeidstilsynet.Common.Altinn.DependencyInjection.DependencyInjectionExtensions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnAppsClient : IAltinnAppsClient
{
    private readonly IAltinnTokenProvider _altinnTokenProvider;
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IOptions<AltinnConfiguration> _config;

    public AltinnAppsClient(
        IHttpClientFactory httpClientFactory,
        IAltinnTokenProvider altinnTokenProvider,
        IOptions<AltinnConfiguration> config
    )
    {
        _altinnTokenProvider = altinnTokenProvider;
        _config = config;
        _httpClient = httpClientFactory.CreateClient(AltinnAppsApiClientKey);
        _jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public async Task<AltinnInstance> CompleteInstance(
        AltinnAppSpecification appSpec,
        InstanceRequest instanceAddress
    )
    {
        var orgId = _config.Value.OrgId;
        var appId = appSpec.AppId;

        var instanceUri = instanceAddress.ToInstanceUri("complete");

        return await _httpClient
                .PostAsJson($"{orgId}/{appId}/{instanceUri}", new { })
                .WithBearerToken(await _altinnTokenProvider.GetToken())
                .ReceiveContent<AltinnInstance>(_jsonSerializerOptions)
            ?? throw new InvalidOperationException("Failed to complete instance");
    }
}
