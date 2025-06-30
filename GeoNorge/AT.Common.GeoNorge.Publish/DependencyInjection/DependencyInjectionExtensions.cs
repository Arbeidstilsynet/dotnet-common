using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace Arbeidstilsynet.Common.GeoNorge.DependencyInjection;

/// <summary>
/// Configuration for GeoNorge API.
/// </summary>
public record GeoNorgeConfig
{
    /// <summary>
    /// Base URL for GeoNorge API. Default is "https://ws.geonorge.no/adresser/v1/".
    /// </summary>
    public required string BaseUrl { get; init; } = "https://ws.geonorge.no/adresser/v1/";
    
    /// <summary>
    /// Cache settings for GeoNorge API. Default is to enable caching with a 1-day expiration time.
    /// </summary>
    public CacheOptions Cache { get; init; } = new CacheOptions(ExpirationTime: TimeSpan.FromDays(1)); 
}

/// <summary>
/// Cache settings for Enhetsregisteret.
/// </summary>
/// <param name="Disabled">If true, disables the caching mechanism. Default: false.</param>
/// <param name="ExpirationTime">If caching is enabled, this specifies how long the cache value should be retained. Default: 1 day.</param>
public record CacheOptions(bool Disabled = false, TimeSpan? ExpirationTime = null);


/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    internal const string ClientKey = "GeoNorgeClient";
    
    /// <summary>
    /// Register GeoNorge services in the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="geoNorgeConfig"></param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddGeoNorge(this IServiceCollection services, GeoNorgeConfig geoNorgeConfig)
    {
        services.AddHttpClient(ClientKey, client =>
        {
            client.BaseAddress = new Uri(geoNorgeConfig.BaseUrl);
        }).AddStandardResilienceHandler();
        
        services.AddMemoryCache();
        services.AddSingleton<IGeoNorge, GeoNorgeClient>();

        return services;
    }
}
