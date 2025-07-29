using System.Text.Json;
using Altinn.App.Core.Infrastructure.Clients.Events;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
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
        _httpClient = httpClientFactory.CreateClient(AltinnAppApiClientKey);
        _jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public async Task<Subscription> Subscribe(SubscriptionRequest request)
    {
        return await _httpClient
                .Post(new Uri("subscriptions", UriKind.Relative).ToString(), request)
                .WithHeader("Authorization", $"Bearer {await _altinnTokenProvider.GetToken()}")
                .ReceiveContent<Subscription>(_jsonSerializerOptions)
            ?? throw new Exception("Failed to subscribe to Altinn");
    }
}
