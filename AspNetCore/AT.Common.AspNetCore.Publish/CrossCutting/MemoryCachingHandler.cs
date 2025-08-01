using Arbeidstilsynet.Common.AspNetCore.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

internal class MemoryCachingHandler(IMemoryCache cache, IOptions<CachingOptions> cachingOptions)
    : DelegatingHandler
{
    private readonly CachingOptions _cachingOptions = cachingOptions.Value;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (request.Method != HttpMethod.Get)
        {
            // Bypass caching for non-GET requests
            return await base.SendAsync(request, cancellationToken);
        }

        var cacheKey = request.GetCacheKey();

        if (
            cacheKey is { Length: > 0 }
            && cache.TryGetCachedResponse(cacheKey, out var cachedResponse)
        )
        {
            return await cachedResponse!.CreateCopy()
                        .WithHeader("X-From-Cache", "true");
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode && cacheKey is { Length: > 0 }
            && response.Content is ByteArrayContent or StringContent)
        {
            // Only cache if content is fully buffered and not null
            cachedResponse = await response.CreateCopy();
            cache.Set(
                cacheKey,
                cachedResponse,
                new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = _cachingOptions.SlidingExpiration,
                    AbsoluteExpirationRelativeToNow = _cachingOptions.AbsoluteExpiration,
                }
            );
        }

        return response;
    }
}

file static class Extensions
{
    public static string? GetCacheKey(this HttpRequestMessage requestMessage)
    {
        if (requestMessage.RequestUri?.ToString() is not { Length: > 0 } cacheKey)
        {
            return null;
        }

        return cacheKey;
    }

    public static bool TryGetCachedResponse(
        this IMemoryCache cache,
        string cacheKey,
        out HttpResponseMessage? cachedResponse
    )
    {
        cachedResponse = default;

        if (
            !cache.TryGetValue(cacheKey, out var value)
            || value is not HttpResponseMessage typedValue
        )
        {
            return false;
        }

        cachedResponse = typedValue;
        return true;
    }

    public static async Task<HttpResponseMessage> CreateCopy(this HttpResponseMessage original)
    {
        var clone = new HttpResponseMessage(original.StatusCode)
        {
            Version = original.Version,
            ReasonPhrase = original.ReasonPhrase,
            RequestMessage = null // Avoid referencing disposed request
        };

        // Copy headers
        foreach (var header in original.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content
        if (original.Content is StringContent stringContent)
        {
            var str = await stringContent.ReadAsStringAsync();
            var encoding = stringContent.Headers.ContentType?.CharSet != null
                ? System.Text.Encoding.GetEncoding(stringContent.Headers.ContentType.CharSet)
                : null;
            var mediaType = stringContent.Headers.ContentType?.MediaType;

            if (encoding == null)
            {
                clone.Content = new StringContent(str);
            }
            else if (mediaType == null)
            {
                clone.Content = new StringContent(str, encoding);
            }
            else
            {
                clone.Content = new StringContent(str, encoding, mediaType);
            }
        }
        else if (original.Content is ByteArrayContent)
        {
            var bytes = await original.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);
        }
        else if (original.Content != null)
        {
            // fallback: buffer as byte array
            var bytes = await original.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);
        }

        // Copy content headers
        if (original.Content != null && clone.Content != null)
        {
            foreach (var header in original.Content.Headers)
            {
                if (!clone.Content.Headers.Contains(header.Key)) // Some content headers are set when setting content
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        return clone;
    }
    
    public static async Task<HttpResponseMessage> WithHeader(
        this Task<HttpResponseMessage> responseTask,
        string headerName,
        string headerValue
    )
    {
        var response = await responseTask;
        
        response.Headers.TryAddWithoutValidation(headerName, headerValue);
        return response;
    }
}
