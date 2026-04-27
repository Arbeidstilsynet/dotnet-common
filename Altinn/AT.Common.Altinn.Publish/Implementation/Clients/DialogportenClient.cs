using System.Text.Json;
using System.Text.Json.Serialization;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using static Arbeidstilsynet.Common.Altinn.DependencyInjection.DependencyInjectionExtensions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class DialogportenClient : IDialogportenClient
{
    private readonly IAltinnTokenProvider _altinnTokenProvider;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public DialogportenClient(
        IHttpClientFactory httpClientFactory,
        IAltinnTokenProvider altinnTokenProvider
    )
    {
        _altinnTokenProvider = altinnTokenProvider;
        _httpClient = httpClientFactory.CreateClient(DialogportenApiClientKey);
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<DialogportenLookupResponse> LookupDialog(string instanceRef)
    {
        return await _httpClient
                .Get("serviceowner/dialoglookup")
                .WithBearerToken(await _altinnTokenProvider.GetToken())
                .WithQueryParameter("instanceRef", instanceRef)
                .ReceiveContent<DialogportenLookupResponse>(_jsonSerializerOptions)
            ?? throw new InvalidOperationException("Failed to look up dialog");
    }
}
