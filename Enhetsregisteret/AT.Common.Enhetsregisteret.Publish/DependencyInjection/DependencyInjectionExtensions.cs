using System.Reflection;
using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;

/// <summary>
/// Configures the <see cref="HttpClient"/> for the Enhetsregisteret API.
/// </summary>
public class EnhetsregisteretConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnhetsregisteretConfig"/> class.
    /// </summary>
    /// <param name="brregApiBaseUrl">BaseUrl for Enhetsregisteret API. Default: "https://data.brreg.no/".</param>
    /// <param name="cacheOptions">Cache settings for Enhetsregisteret.
    /// </param>
    public EnhetsregisteretConfig(
        string? brregApiBaseUrl = null,
        CacheOptions? cacheOptions = null,
        string? embeddedOrgDataResourceName = null
    )
    {
        BrregApiBaseUrl = brregApiBaseUrl ?? "https://data.brreg.no/";
        CacheOptions = new CacheOptions { Disabled = cacheOptions?.Disabled ?? false };
        EmbeddedOrgDataResourceName =
            embeddedOrgDataResourceName
            ?? "Arbeidstilsynet.Common.Enhetsregisteret.Data.orgdata.json";
    }

    /// <summary>
    /// BaseUrl for Enhetsregisteret API.
    /// </summary>
    public string BrregApiBaseUrl { get; set; }

    /// <summary>
    /// Settings for caching mechanism.
    /// </summary>
    public CacheOptions CacheOptions { get; set; }

    /// <summary>
    /// Resource name for the embedded test resource data set.
    /// </summary>
    public string? EmbeddedOrgDataResourceName { get; set; }
}

/// <summary>
/// Cache settings for Enhetsregisteret.
/// </summary>
/// <param name="Disabled">If true, disables the caching mechanism. Default: false.</param>
public record CacheOptions(bool Disabled = false);

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    internal const string Clientkey = "EnhetsregisteretClient";

    /// <summary>
    /// Registers an implementation of IEnhetsregisteret in the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="configure">Configure the client.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddEnhetsregisteret(
        this IServiceCollection services,
        IWebHostEnvironment webHostEnvironment,
        Action<EnhetsregisteretConfig>? configure = null
    )
    {
        var config = new EnhetsregisteretConfig();

        configure?.Invoke(config);

        services.AddServices(webHostEnvironment, config);

        return services;
    }

    /// <summary>
    /// Registers an implementation of IEnhetsregisteret in the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="config">Configuration for the client. If null, a default configuration is used.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddEnhetsregisteret(
        this IServiceCollection services,
        IWebHostEnvironment webHostEnvironment,
        EnhetsregisteretConfig? config = null
    )
    {
        config ??= new EnhetsregisteretConfig();

        services.AddServices(webHostEnvironment, config);

        return services;
    }

    private static void AddServices(
        this IServiceCollection services,
        IWebHostEnvironment webHostEnvironment,
        EnhetsregisteretConfig config
    )
    {
        services.AddMapper();
        services.AddSingleton(config!);

        if (webHostEnvironment.IsProduction() || config.BrregApiBaseUrl != null)
        {
            services
                .AddHttpClient(
                    Clientkey,
                    httpClient =>
                    {
                        httpClient.BaseAddress = new Uri(config.BrregApiBaseUrl);
                    }
                )
                .AddStandardResilienceHandler();

            services.AddMemoryCache();
            services.AddTransient<IEnhetsregisteret, EnhetsregisteretClient>();
        }
        else
        {
            services.AddTransient<IEnhetsregisteret, TenorClient>();
        }
    }

    internal static IServiceCollection AddMapper(this IServiceCollection services)
    {
        var existingConfig = services
            .Select(s => s.ImplementationInstance)
            .OfType<TypeAdapterConfig>()
            .FirstOrDefault();

        if (existingConfig == null)
        {
            var config = new TypeAdapterConfig()
            {
                RequireExplicitMapping = false,
                RequireDestinationMemberSource = true,
            };
            config.Scan(Assembly.GetExecutingAssembly());
            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();
        }
        else
        {
            existingConfig.Scan(Assembly.GetExecutingAssembly());
        }
        return services;
    }
}
