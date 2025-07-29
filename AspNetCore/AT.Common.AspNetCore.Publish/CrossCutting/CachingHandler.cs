using Microsoft.Extensions.Caching.Memory;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

internal class MemoryCachingHandler(IMemoryCache cache) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
        {            
            // Bypass caching for non-GET requests
            return await base.SendAsync(request, cancellationToken);
        }
        
        var cacheKey = request.GetCacheKey();
        
        if (cacheKey is { Length: > 0 } 
            && cache.TryGetCachedResponse(cacheKey, out var cachedResponse))
        {
            return cachedResponse;
        }
        
        var response = await base.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode && cacheKey is { Length: > 0 })
        {
            // Clone the response to allow it to be read multiple times
            var cachedClonedResponse = await response.Clone();
            cache.Set(cacheKey, cachedClonedResponse);
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
        out HttpResponseMessage cachedResponse)
    {
        cachedResponse = null!;

        if (!cache.TryGetValue(cacheKey, out var value) || value is not HttpResponseMessage typedValue)
        {
            return false;
        } 
        
        cachedResponse = typedValue;
        return true;

    }
    
    public static async Task<HttpResponseMessage> Clone(this HttpResponseMessage original)
    {
        var cloned = new HttpResponseMessage(original.StatusCode);
        
        foreach (var header in original.Headers)
        {
            cloned.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        cloned.Content = new StreamContent(await original.Content.ReadAsStreamAsync());
        foreach (var header in original.Content.Headers)
        {
            cloned.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        
        cloned.RequestMessage = original.RequestMessage;
        cloned.Version = original.Version;
        
        return cloned;
    }
}