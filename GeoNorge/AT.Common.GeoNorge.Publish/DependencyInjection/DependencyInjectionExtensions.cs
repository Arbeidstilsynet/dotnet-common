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
    /// Base URL for GeoNorge API. Default is "https://ws.geonorge.no/".
    /// </summary>
    public string BaseUrl { get; init; } = "https://ws.geonorge.no/";
}

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
    public static IServiceCollection AddGeoNorge(
        this IServiceCollection services,
        GeoNorgeConfig? geoNorgeConfig = null
    )
    {
        geoNorgeConfig ??= new GeoNorgeConfig();

        services
            .AddHttpClient(
                ClientKey,
                client =>
                {
                    client.BaseAddress = new Uri(geoNorgeConfig.BaseUrl);
                }
            )
            .AddStandardResilienceHandler();

        services.AddSingleton<IGeoNorge, GeoNorgeClient>();

        return services;
    }
}
