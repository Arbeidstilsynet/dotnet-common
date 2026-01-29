using System.Text.Json;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using static Arbeidstilsynet.Common.Altinn.DependencyInjection.DependencyInjectionExtensions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnEventsClient : IAltinnEventsClient
{
    private readonly IAltinnTokenProvider _altinnTokenProvider;
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AltinnEventsClient(
        IHttpClientFactory httpClientFactory,
        IAltinnTokenProvider altinnTokenProvider
    )
    {
        _altinnTokenProvider = altinnTokenProvider;
        _httpClient = httpClientFactory.CreateClient(AltinnEventsApiClientKey);
        _jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public async Task<AltinnSubscription> GetAltinnSubscription(int subscriptionId)
    {
        return await _httpClient
            .Get($"subscriptions/{subscriptionId}")
            .WithBearerToken(await _altinnTokenProvider.GetToken())
            .ReceiveContent<AltinnSubscription>(_jsonSerializerOptions);
    }

    public async Task<AltinnSubscription> Subscribe(AltinnSubscriptionRequest request)
    {
        return await _httpClient
                .PostAsJson("subscriptions", request)
                .WithBearerToken(await _altinnTokenProvider.GetToken())
                .ReceiveContent<AltinnSubscription>(_jsonSerializerOptions)
            ?? throw new Exception("Failed to subscribe to Altinn");
    }

    public async Task<HttpResponseMessage> Unsubscribe(int subscriptionId)
    {
        return await _httpClient
                .Delete($"subscriptions/{subscriptionId}")
                .WithBearerToken(await _altinnTokenProvider.GetToken())
                .Send() ?? throw new Exception("Failed to unsubscribe from Altinn");
    }
}
