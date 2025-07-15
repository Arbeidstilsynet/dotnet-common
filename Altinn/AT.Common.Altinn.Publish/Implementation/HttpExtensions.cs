using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.App.Core.Models;
using Arbeidstilsynet.Common.Altinn.Model;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;

namespace Arbeidstilsynet.Common.Altinn.Implementation;

internal static class HttpExtensions
{
    public static HttpRequestBuilder Post<TContent>(
        this HttpClient client,
        string resource,
        TContent content
    )
    {
        return new HttpRequestBuilder(
            client,
            new HttpRequestMessage()
            {
                RequestUri = new Uri(resource, UriKind.Relative),
                Method = HttpMethod.Post,
                Content = JsonContent.Create(content),
            }
        );
    }

    public static HttpRequestBuilder Get(this HttpClient client, string resource)
    {
        return new HttpRequestBuilder(
            client,
            new HttpRequestMessage()
            {
                RequestUri = new Uri(resource, UriKind.Relative),
                Method = HttpMethod.Get,
            }
        );
    }

    public static async Task<TResponse?> ReceiveContent<TResponse>(
        this HttpRequestBuilder requestBuilder,
        JsonSerializerOptions? options = null
    )
    {
        var response = await requestBuilder.Send();

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(options);
    }

    public static async Task<Stream> ReceiveStream(this HttpRequestBuilder requestBuilder)
    {
        var response = await requestBuilder.Send();

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }

    public static Uri ToInstanceUri(
        this InstanceRequest instanceAddress,
        string? pathAppendix = null
    )
    {
        var baseUri =
            $"instances/{instanceAddress.InstanceOwnerPartyId}/{instanceAddress.InstanceGuid}";
        if (!string.IsNullOrEmpty(pathAppendix))
        {
            baseUri += pathAppendix.StartsWith('/') ? pathAppendix : $"/{pathAppendix}";
        }
        return new Uri(baseUri, UriKind.Relative);
    }

    public static Uri MakeRelativeOrThrow(this Uri? baseUri, Uri uri)
    {
        ArgumentNullException.ThrowIfNull(baseUri);

        var relativeUri = baseUri.MakeRelativeUri(uri);

        if (relativeUri.IsAbsoluteUri)
        {
            throw new InvalidOperationException(
                $"BaseUri:<{baseUri}> and ResourceUri:<{uri}> have different roots"
            );
        }

        return relativeUri;
    }
}
