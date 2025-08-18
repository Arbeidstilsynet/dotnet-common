using System.Net.Http.Json;
using System.Text.Json;
using Altinn.App.Core.Models;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;

namespace Arbeidstilsynet.Common.Altinn.Implementation;

internal static class HttpExtensions
{
    public static IHttpRequestBuilder WithBearerToken(
        this IHttpRequestBuilder requestBuilder,
        string token
    )
    {
        return requestBuilder.WithHeader("Authorization", $"Bearer {token}");
    }

    public static IHttpRequestBuilder PostAsJson<T>(
        this HttpClient client,
        string resource,
        T content
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

    public static IHttpRequestBuilder PostAsJson<T>(this HttpClient client, Uri uri, T content)
    {
        return new HttpRequestBuilder(
            client,
            new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Content = JsonContent.Create(content),
            }
        );
    }

    public static IHttpRequestBuilder Post(
        this HttpClient client,
        string resource,
        HttpContent content
    )
    {
        return new HttpRequestBuilder(
            client,
            new HttpRequestMessage()
            {
                RequestUri = new Uri(resource, UriKind.Relative),
                Method = HttpMethod.Post,
                Content = content,
            }
        );
    }

    public static IHttpRequestBuilder Post(this HttpClient client, Uri uri, HttpContent content)
    {
        return new HttpRequestBuilder(
            client,
            new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Content = content,
            }
        );
    }

    public static IHttpRequestBuilder Get(this HttpClient client, string resource)
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

    public static IHttpRequestBuilder Get(this HttpClient client, Uri uri)
    {
        return new HttpRequestBuilder(
            client,
            new HttpRequestMessage() { RequestUri = uri, Method = HttpMethod.Get }
        );
    }

    public static async Task<TResponse?> ReceiveContent<TResponse>(
        this IHttpRequestBuilder requestBuilder,
        JsonSerializerOptions? options = null
    )
    {
        var response = await requestBuilder.Send();

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(options);
    }

    public static async Task<Stream> ReceiveStream(this IHttpRequestBuilder requestBuilder)
    {
        var response = await requestBuilder.Send();

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }

    public static async Task<string> ReceiveString(this IHttpRequestBuilder requestBuilder)
    {
        var response = await requestBuilder.Send();

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public static Uri ToInstanceUri(
        this InstanceRequest instanceAddress,
        string? pathAppendix = null
    )
    {
        var uri =
            $"instances/{instanceAddress.InstanceOwnerPartyId}/{instanceAddress.InstanceGuid}";
        if (!string.IsNullOrEmpty(pathAppendix))
        {
            uri += pathAppendix.StartsWith('/') ? pathAppendix : $"/{pathAppendix}";
        }
        return new Uri(uri, UriKind.Relative);
    }

    public static Uri ToInstanceUri(this CloudEvent cloudEvent)
    {
        try
        {
            var sourcePath = cloudEvent.Source.PathAndQuery;
            var uri = sourcePath[sourcePath.IndexOf("instances", StringComparison.Ordinal)..];
            var queryIndex = uri.IndexOf('?');
            if (queryIndex >= 0)
            {
                uri = uri[..queryIndex];
            }
            var trimmedUri = string.Join("/", uri.Split("/").Take(3));
            return new Uri(trimmedUri, UriKind.Relative);
        }
        catch (Exception e)
        {
            throw new AltinnEventSourceParseException(
                $"Could not extract the instance identifier for the provided Source URL by an altinn cloud event. The source url was {cloudEvent.Source?.AbsoluteUri}",
                e
            );
        }
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

    public static IHttpRequestBuilder ApplyInstanceQueryParameters(
        this IHttpRequestBuilder httpRequestBuilder,
        InstanceQueryParameters queryParameters
    )
    {
        foreach (var (name, value) in queryParameters.GetQueryParameters())
        {
            httpRequestBuilder.WithQueryParameter(name, value);
        }

        foreach (var (name, value) in queryParameters.GetHeaderParameters())
        {
            httpRequestBuilder.WithHeader(name, value);
        }

        return httpRequestBuilder;
    }
}
