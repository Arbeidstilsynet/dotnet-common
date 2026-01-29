using System.Text.Json;
using System.Text.Json.Serialization;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using static Arbeidstilsynet.Common.Altinn.DependencyInjection.DependencyInjectionExtensions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnStorageClient : IAltinnStorageClient
{
    private readonly IAltinnTokenProvider _altinnTokenProvider;
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AltinnStorageClient(
        IHttpClientFactory httpClientFactory,
        IAltinnTokenProvider altinnTokenProvider
    )
    {
        _altinnTokenProvider = altinnTokenProvider;
        _httpClient = httpClientFactory.CreateClient(AltinnStorageApiClientKey);
        _jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<Stream> GetInstanceData(InstanceDataRequest request)
    {
        var uri = request.InstanceRequest.ToInstanceUri($"data/{request.DataId}");

        return await _httpClient
                .Get(uri)
                .WithBearerToken(await _altinnTokenProvider.GetToken())
                .ReceiveStream() ?? throw new Exception("Failed to get instance data");
    }

    public async Task<Stream> GetInstanceData(Uri absoluteUri)
    {
        var url = _httpClient.BaseAddress.MakeRelativeOrThrow(absoluteUri).ToString();

        return await _httpClient
            .Get(url)
            .WithBearerToken(await _altinnTokenProvider.GetToken())
            .ReceiveStream();
    }

    public async Task<AltinnInstance> GetInstance(InstanceRequest instanceAddress)
    {
        return await _httpClient
                .Get(instanceAddress.ToInstanceUri())
                .WithBearerToken(await _altinnTokenProvider.GetToken())
                .ReceiveContent<AltinnInstance>(_jsonSerializerOptions)
            ?? throw new Exception("Failed to get instance");
    }

    public async Task<AltinnInstance> GetInstance(AltinnCloudEvent cloudEvent)
    {
        return await _httpClient
                .Get(cloudEvent.ToInstanceUri())
                .WithBearerToken(await _altinnTokenProvider.GetToken())
                .ReceiveContent<AltinnInstance>(_jsonSerializerOptions)
            ?? throw new Exception("Failed to get instance");
    }

    public async Task<AltinnQueryResponse<AltinnInstance>> GetInstances(
        InstanceQueryParameters queryParameters
    )
    {
        return await _httpClient
                .Get("instances")
                .ApplyInstanceQueryParameters(queryParameters)
                .WithBearerToken(await _altinnTokenProvider.GetToken())
                .ReceiveContent<AltinnQueryResponse<AltinnInstance>>(_jsonSerializerOptions)
            ?? throw new Exception("Failed to get instance");
    }
}
