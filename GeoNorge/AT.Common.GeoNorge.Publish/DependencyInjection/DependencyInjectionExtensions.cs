using Arbeidstilsynet.Common.GeoNorge.Adresser;
using Arbeidstilsynet.Common.GeoNorge.Eiendom;
using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Arbeidstilsynet.Common.GeoNorge.KommuneInfo;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Arbeidstilsynet.Shared.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;

namespace Arbeidstilsynet.Common.GeoNorge.DependencyInjection;

/// <summary>The implementation to use for GeoNorge.</summary>
public enum GeoNorgeEnvironment
{
    /// <summary>The public Kartverket APIs. There is no separate public test target.</summary>
    Production,

    /// <summary>Offline convenience-interface stubs; generated clients are not registered.</summary>
    Stub,
}

/// <summary>
/// Configuration for GeoNorge API.
/// </summary>
public record GeoNorgeConfig
{
    /// <summary>
    /// Defaults to Production in Production and Staging, and Stub in Development.
    /// Required for other host environments. Production only permits the Production target.
    /// </summary>
    public GeoNorgeEnvironment? Environment { get; init; }

    /// <summary>
    /// Base URL for the GeoNorge APIs. Default is "https://ws.geonorge.no/".
    /// <br/>
    /// The API-specific base paths ("adresser/v1" and "kommuneinfo/v1") are appended automatically.
    /// Setting this selects real address and municipality adapters even when the target is Stub.
    /// Non-default URLs are rejected in Production and logged at startup in Staging.
    /// </summary>
    public string? BaseUrl { get; init; }

    /// <summary>
    /// Base URL for cadastral property searches. Default is "https://api.kartverket.no/".
    /// The API-specific base path "eiendom/v1" is appended automatically.
    /// This setting is independent of <see cref="BaseUrl"/>.
    /// Setting this selects the real property adapter even when the target is Stub.
    /// Non-default URLs are rejected in Production and logged at startup in Staging.
    /// </summary>
    public string? PropertyBaseUrl { get; init; }

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
    internal const string EiendomHttpClientName = "GeoNorgeEiendomClient";

    private const string AdresserBasePath = "adresser/v1";
    private const string KommuneInfoBasePath = "kommuneinfo/v1";
    private const string EiendomBasePath = "eiendom/v1";
    private const string DefaultBaseUrl = "https://ws.geonorge.no/";
    private const string DefaultPropertyBaseUrl = "https://api.kartverket.no/";

    /// <summary>
    /// Register GeoNorge services in the provided <see cref="IServiceCollection"/>.
    /// <br/>
    /// Exposes the Kiota-generated <see cref="AdresserClient"/>, <see cref="KommuneInfoClient"/>
    /// and <see cref="EiendomClient"/> for local adaptation, as well as the <see cref="IAddressSearch"/>,
    /// <see cref="IFylkeKommuneApi"/> and <see cref="IPropertySearch"/> ports through a small,
    /// task-oriented API.
    /// </summary>
    /// <remarks>
    /// Development defaults to offline port implementations. Generated clients are only registered
    /// for APIs configured with a real target or an explicit base URL.
    /// </remarks>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="hostEnvironment">The application environment controlling defaults and allowed endpoints.</param>
    /// <param name="geoNorgeConfig">Optional configuration. Uses defaults if not specified.</param>
    /// <param name="configureResilience">
    /// Optional callback for customizing the standard HTTP resilience handler applied to all clients.
    /// </param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddGeoNorge(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment,
        GeoNorgeConfig? geoNorgeConfig = null,
        Action<HttpStandardResilienceOptions>? configureResilience = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        geoNorgeConfig ??= new GeoNorgeConfig();

        var target = EnvironmentClientPolicy.ResolveTarget(
            hostEnvironment,
            geoNorgeConfig.Environment,
            GeoNorgeEnvironment.Production,
            GeoNorgeEnvironment.Production,
            GeoNorgeEnvironment.Stub
        );
        var baseUrl = EnvironmentClientPolicy.ResolveUrl(
            hostEnvironment,
            geoNorgeConfig.BaseUrl,
            DefaultBaseUrl
        );
        var propertyBaseUrl = EnvironmentClientPolicy.ResolveUrl(
            hostEnvironment,
            geoNorgeConfig.PropertyBaseUrl,
            DefaultPropertyBaseUrl
        );
        EnvironmentClientPolicy.WarnOnStagingOverrides(
            services,
            hostEnvironment,
            "GeoNorge",
            (baseUrl, DefaultBaseUrl),
            (propertyBaseUrl, DefaultPropertyBaseUrl)
        );

        services.AddSingleton(geoNorgeConfig);

        if (target == GeoNorgeEnvironment.Stub && geoNorgeConfig.BaseUrl is null)
        {
            services.AddScoped<IAddressSearch, AddressSearchStub>();
            services.AddScoped<IFylkeKommuneApi>(_ =>
            {
                IFylkeKommuneApi stub = new FylkeKommuneStub();
                return geoNorgeConfig.UseApproximateSvalbardAndJanMayen
                    ? new ApproximateSvalbardAndJanMayenFylkeKommuneApi(stub)
                    : stub;
            });
        }
        else
        {
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
                requestAdapter.BaseUrl = CombineBaseUrl(baseUrl.AbsoluteUri, AdresserBasePath);
                return new AdresserClient(requestAdapter);
            });

            services.AddScoped(serviceProvider =>
            {
                var requestAdapter =
                    serviceProvider.GetRequiredService<KommuneInfoRequestAdapter>();
                requestAdapter.BaseUrl = CombineBaseUrl(baseUrl.AbsoluteUri, KommuneInfoBasePath);
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
        }

        if (target == GeoNorgeEnvironment.Stub && geoNorgeConfig.PropertyBaseUrl is null)
        {
            services.AddScoped<IPropertySearch, PropertySearchStub>();
        }
        else
        {
            services
                .AddHttpClient(EiendomHttpClientName)
                .AddStandardResilienceHandler(options => configureResilience?.Invoke(options));
            services.AddScoped<EiendomRequestAdapter>();
            services.AddScoped(serviceProvider =>
            {
                var requestAdapter = serviceProvider.GetRequiredService<EiendomRequestAdapter>();
                requestAdapter.BaseUrl = CombineBaseUrl(
                    propertyBaseUrl.AbsoluteUri,
                    EiendomBasePath
                );
                return new EiendomClient(requestAdapter);
            });
            services.AddScoped<IPropertySearch, PropertySearchClient>();
        }

        return services;
    }

    private static string CombineBaseUrl(string baseUrl, string basePath)
    {
        return $"{baseUrl.TrimEnd('/')}/{basePath}";
    }
}
