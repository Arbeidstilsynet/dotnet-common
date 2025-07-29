using System.Net.Http.Json;

namespace Arbeidstilsynet.Common.MeldingerReceiver.Implementation;

internal class MeldingerReceiverClient
{
    private readonly HttpClient _httpClient;

    public MeldingerReceiverClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(
            DependencyInjection.DependencyInjectionExtensions.MeldingerReceiverApiClientKey
        );
    }

    public Task GetDocuments(Guid meldingId)
    {
        return _httpClient.GetFromJsonAsync<string>($"{meldingId}/documents");
    }

    public Task GetDocument(Guid meldingId, Guid documentId)
    {
        return _httpClient.GetFromJsonAsync<string>($"{meldingId}/documents/{documentId}");
    }
}
