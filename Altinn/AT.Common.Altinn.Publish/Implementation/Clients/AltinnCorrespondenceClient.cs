using System.Text.Json;
using System.Text.Json.Serialization;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Microsoft.AspNetCore.Http;
using static Arbeidstilsynet.Common.Altinn.DependencyInjection.DependencyInjectionExtensions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnCorrespondenceClient : IAltinnCorrespondenceClient
{
    private readonly IAltinnTokenProvider _altinnTokenProvider;
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AltinnCorrespondenceClient(
        IHttpClientFactory httpClientFactory,
        IAltinnTokenProvider altinnTokenProvider
    )
    {
        _altinnTokenProvider = altinnTokenProvider;
        _httpClient = httpClientFactory.CreateClient(AltinnCorrespondenceApiClientKey);
        _jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<AltinnCorrespondenceOverview> GetCorrespondence(Guid guid)
    {
        return await _httpClient
                .Get($"correspondence/{guid}")
                .WithBearerToken(await _altinnTokenProvider.GetToken())
                .ReceiveContent<AltinnCorrespondenceOverview>(_jsonSerializerOptions)
            ?? throw new InvalidOperationException("Failed to retrieve correspondence");
    }

    public async Task<CorrespondenceResponse> InitializeCorrespondence(
        InitializeCorrespondences request,
        List<IFormFile>? attachments
    )
    {
        var token = await _altinnTokenProvider.GetToken();

        if (attachments != null)
        {
            var content = request.ToMultipartFormData(attachments);
            return await _httpClient
                    .Post("correspondence/upload", content)
                    .WithBearerToken(token)
                    .ReceiveContent<CorrespondenceResponse>(_jsonSerializerOptions)
                ?? throw new InvalidOperationException("Failed to send correspondence");
        }

        return await _httpClient
                .PostAsJson("correspondence", request)
                .WithBearerToken(token)
                .ReceiveContent<CorrespondenceResponse>(_jsonSerializerOptions)
            ?? throw new InvalidOperationException("Failed to send correspondence");
    }
}
