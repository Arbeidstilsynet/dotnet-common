using System.Text.Json;
using Altinn.App.Core.Infrastructure.Clients.Events;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using static Arbeidstilsynet.Common.Altinn.DependencyInjection.DependencyInjectionExtensions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnAppsClient : IAltinnAppsClient
{
    private readonly IAltinnTokenProvider _altinnTokenProvider;
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AltinnAppsClient(
        IHttpClientFactory httpClientFactory,
        IAltinnTokenProvider altinnTokenProvider
    )
    {
        _altinnTokenProvider = altinnTokenProvider;
        _httpClient = httpClientFactory.CreateClient(AltinnAppsApiClientKey);
        _jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public async Task<Instance> CompleteInstance(string appId, InstanceRequest instanceAddress)
    {
        var instanceUri = instanceAddress.ToInstanceUri("complete");

        return await _httpClient
                .PostAsJson($"{AltinnOrgIdentifier}/{appId}/{instanceUri}", new { })
                .WithBearerToken(await _altinnTokenProvider.GetToken())
                .ReceiveContent<Instance>(_jsonSerializerOptions)
            ?? throw new Exception("Failed to complete instance");
    }
}
