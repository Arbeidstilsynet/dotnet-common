using Arbeidstilsynet.Common.GeoNorge.Adresser;
using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Arbeidstilsynet.Common.GeoNorge.KommuneInfo;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace Arbeidstilsynet.Common.GeoNorge.DependencyInjection;

/// <summary>
/// Configuration for GeoNorge API.
/// </summary>
public record GeoNorgeConfig
{
    /// <summary>
    /// Base URL for the GeoNorge APIs. Default is "https://ws.geonorge.no/".
    /// <br/>
    /// The API-specific base paths ("adresser/v1" and "kommuneinfo/v1") are appended automatically.
    /// </summary>
    public string BaseUrl { get; init; } = "https://ws.geonorge.no/";

    /// <summary>
    /// If true, uses an approximate method for determining if coordinates are within Svalbard and Jan Mayen.
    /// </summary>
    /// <remarks>
    /// This is a workaround for the fact that the GeoNorge API does not include Svalbard and Jan Mayen in its fylke and kommune data.
    /// </remarks>
    public bool UseApproximateSvalbardAndJanMayen { get; init; } = false;
}

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    internal const string AdresserHttpClientName = "GeoNorgeAdresserClient";
    internal const string KommuneInfoHttpClientName = "GeoNorgeKommuneInfoClient";

    private const string AdresserBasePath = "adresser/v1";
    private const string KommuneInfoBasePath = "kommuneinfo/v1";

    /// <summary>
    /// Register GeoNorge services in the provided <see cref="IServiceCollection"/>.
    /// <br/>
    /// Exposes the Kiota-generated <see cref="AdresserClient"/> and <see cref="KommuneInfoClient"/>
    /// for local adaptation, as well as the domain-oriented <see cref="IAddressSearch"/> and
    /// <see cref="IFylkeKommuneApi"/> ports that map the generated models to the package's own models.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="geoNorgeConfig">Optional configuration. Uses defaults if not specified.</param>
    /// <param name="configureResilience">
    /// Optional callback for customizing the standard HTTP resilience handler applied to both clients.
    /// </param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddGeoNorge(
        this IServiceCollection services,
        GeoNorgeConfig? geoNorgeConfig = null,
        Action<HttpStandardResilienceOptions>? configureResilience = null
    )
    {
        geoNorgeConfig ??= new GeoNorgeConfig();

        services.AddSingleton(geoNorgeConfig);

        services
            .AddHttpClient(AdresserHttpClientName)
            .AddStandardResilienceHandler(options => configureResilience?.Invoke(options));

        services
            .AddHttpClient(KommuneInfoHttpClientName)
            .AddStandardResilienceHandler(options => configureResilience?.Invoke(options));

        services.AddScoped<AdresserRequestAdapter>();
        services.AddScoped<KommuneInfoRequestAdapter>();

        services.AddScoped(serviceProvider =>
        {
            var requestAdapter = serviceProvider.GetRequiredService<AdresserRequestAdapter>();
            requestAdapter.BaseUrl = CombineBaseUrl(geoNorgeConfig.BaseUrl, AdresserBasePath);
            return new AdresserClient(requestAdapter);
        });

        services.AddScoped(serviceProvider =>
        {
            var requestAdapter = serviceProvider.GetRequiredService<KommuneInfoRequestAdapter>();
            requestAdapter.BaseUrl = CombineBaseUrl(geoNorgeConfig.BaseUrl, KommuneInfoBasePath);
            return new KommuneInfoClient(requestAdapter);
        });

        services.AddScoped<IAddressSearch, AddressSearchClient>();

        services.AddScoped<FylkeKommuneClient>();
        services.AddScoped<IFylkeKommuneApi>(serviceProvider =>
        {
            var fylkeKommuneClient = serviceProvider.GetRequiredService<FylkeKommuneClient>();

            return geoNorgeConfig.UseApproximateSvalbardAndJanMayen
                ? new ApproximateSvalbardAndJanMayenFylkeKommuneApi(fylkeKommuneClient)
                : fylkeKommuneClient;
        });

        return services;
    }

    private static string CombineBaseUrl(string baseUrl, string basePath)
    {
        return $"{baseUrl.TrimEnd('/')}/{basePath}";
    }
}
