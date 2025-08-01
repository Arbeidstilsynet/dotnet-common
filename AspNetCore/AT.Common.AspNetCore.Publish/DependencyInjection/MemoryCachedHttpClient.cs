using Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.AspNetCore.DependencyInjection;

public record CachingOptions
{
    /// <summary>
    /// The sliding expiration for cached responses.
    /// </summary>
    public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The absolute expiration for cached responses.
    /// </summary>
    public TimeSpan AbsoluteExpiration { get; set; } = TimeSpan.FromHours(1);
};

public static class MemoryCachedHttpClient
{
    /// <summary>
    /// Adds a cached HTTP client to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="name"></param>
    /// <param name="configureCachingOptions"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddMemoryCachedClient(
        this IServiceCollection services,
        string name,
        Action<CachingOptions>? configureCachingOptions = null
    )
    {
        var options = new CachingOptions();

        configureCachingOptions?.Invoke(options);

        services.TryAddSingleton(Options.Create(options));
        services.AddMemoryCache();
        services.TryAddTransient<MemoryCachingHandler>();

        return services.AddHttpClient(name).AddHttpMessageHandler<MemoryCachingHandler>();
    }

    /// <summary>
    /// Adds a cached HTTP client to the service collection with a custom configuration action.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="name"></param>
    /// <param name="configureClient"></param>
    /// <param name="configureCachingOptions"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddMemoryCachedClient(
        this IServiceCollection services,
        string name,
        Action<HttpClient> configureClient,
        Action<CachingOptions>? configureCachingOptions = null
    )
    {
        var clientBuilder = services.AddMemoryCachedClient(name, configureCachingOptions);
        clientBuilder.ConfigureHttpClient(configureClient);
        return clientBuilder;
    }

    /// <summary>
    /// Adds a cached HTTP client to the service collection with a custom configuration action that receives the service provider.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="name"></param>
    /// <param name="configureClient"></param>
    /// <param name="configureCachingOptions"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddMemoryCachedClient(
        this IServiceCollection services,
        string name,
        Action<IServiceProvider, HttpClient> configureClient,
        Action<CachingOptions>? configureCachingOptions = null
    )
    {
        var clientBuilder = services.AddMemoryCachedClient(name, configureCachingOptions);
        clientBuilder.ConfigureHttpClient(configureClient);
        return clientBuilder;
    }
}
