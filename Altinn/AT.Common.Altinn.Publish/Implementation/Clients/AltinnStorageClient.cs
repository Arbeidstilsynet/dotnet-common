using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.App.Core.Models;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
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

    public async Task<Instance> GetInstance(InstanceRequest instanceAddress)
    {
        return await _httpClient
                .Get(instanceAddress.ToInstanceUri().ToString())
                .WithHeader("Authorization", $"Bearer {await _altinnTokenProvider.GetToken()}")
                .ReceiveContent<Instance>(_jsonSerializerOptions)
            ?? throw new Exception("Failed to get instance");
    }

    public async Task<Instance> GetInstance(CloudEvent cloudEvent)
    {
        return await _httpClient
                .Get(cloudEvent.ToInstanceUri().ToString())
                .WithHeader("Authorization", $"Bearer {await _altinnTokenProvider.GetToken()}")
                .ReceiveContent<Instance>(_jsonSerializerOptions)
            ?? throw new Exception("Failed to get instance");
    }

    public async Task<Instance> CompleteInstance(InstanceRequest instanceAddress)
    {
        var url = instanceAddress.ToInstanceUri("complete").ToString();

        return await _httpClient
                .Post(url, new { })
                .WithHeader("Authorization", $"Bearer {await _altinnTokenProvider.GetToken()}")
                .ReceiveContent<Instance>(_jsonSerializerOptions)
            ?? throw new Exception("Failed to complete instance");
    }

    public async Task<Stream> GetInstanceData(InstanceDataRequest request)
    {
        var uri = request.InstanceRequest.ToInstanceUri($"data/{request.DataId}");

        return await _httpClient
                .Get(uri.ToString())
                .WithHeader("Authorization", $"Bearer {await _altinnTokenProvider.GetToken()}")
                .ReceiveStream() ?? throw new Exception("Failed to get instance data");
    }

    public async Task<Stream> GetInstanceData(Uri absoluteUri)
    {
        var url = _httpClient.BaseAddress.MakeRelativeOrThrow(absoluteUri).ToString();

        return await _httpClient
            .Get(url)
            .WithHeader("Authorization", $"Bearer {await _altinnTokenProvider.GetToken()}")
            .ReceiveStream();
    }

    public async Task<QueryResponse<Instance>> GetInstances(InstanceQueryParameters queryParameters)
    {
        return await _httpClient
                .Get(new Uri("instances", UriKind.Relative).ToString())
                .ApplyInstanceQueryParameters(queryParameters)
                .WithHeader("Authorization", $"Bearer {await _altinnTokenProvider.GetToken()}")
                .ReceiveContent<QueryResponse<Instance>>(_jsonSerializerOptions)
            ?? throw new Exception("Failed to get instance");
    }
}
