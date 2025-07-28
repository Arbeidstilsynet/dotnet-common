using System.Collections;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.App.Core.Models;
using Arbeidstilsynet.Common.Altinn.Model;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;

namespace Arbeidstilsynet.Common.Altinn.Implementation;

internal static class HttpExtensions
{
    public static IHttpRequestBuilder Post<TContent>(
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
            var uri = sourcePath[sourcePath.IndexOf("instances")..];
            var queryIndex = uri.IndexOf('?');
            if (queryIndex >= 0)
            {
                uri = uri.Substring(0, queryIndex);
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
        var selectedParameters = queryParameters
            .GetType()
            .GetProperties()
            .Select(s => (s, s.GetValue(queryParameters)))
            .Where((s) => s.Item2 != null);
        
        foreach (var (propertyInfo, parameterValue) in selectedParameters)
        {
            if (parameterValue is null)
            {
                continue;
            }
            
            if (propertyInfo.GetCustomAttribute<MappedQueryParameterAttribute>() is { QueryParameterName: {Length: > 0} queryParameterName })
            {
                if (parameterValue is ICollection<object> collection)
                {
                    foreach (var item in collection)
                    {
                        httpRequestBuilder = httpRequestBuilder.WithQueryParameter(
                            queryParameterName,
                            item.ToString()!
                        );
                    }
                }
                else
                {
                    httpRequestBuilder = httpRequestBuilder.WithQueryParameter(
                        queryParameterName,
                        parameterValue.ToString()
                    );
                    
                }
            }

            if (propertyInfo.GetCustomAttribute<MappedRequestHeaderParameterAttribute>() is { HeaderParameterName: { Length: > 0 } headerParameterName })
            {
                httpRequestBuilder = httpRequestBuilder.WithHeader(
                    headerParameterName,
                    parameterValue.ToString()!
                );
            }
        }
        return httpRequestBuilder;
    }
}
