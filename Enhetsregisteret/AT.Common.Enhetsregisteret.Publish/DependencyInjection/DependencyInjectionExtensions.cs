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
    /// BaseUrl for Enhetsregisteret API.
    /// </summary>
    public string BrregApiBaseUrl { get; set; }

    /// <summary>
    /// Settings for caching mechanism.
    /// </summary>
    public CacheOptions CacheOptions { get; set; }
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
    /// <param name="webHostEnvironment">the environment the application runs in</param>
    /// <param name="configure">Configure the client.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddEnhetsregisteret(
        this IServiceCollection services,
        IWebHostEnvironment webHostEnvironment,
        Action<EnhetsregisteretConfig>? configure = null
    )
    {
        var config = new EnhetsregisteretConfig()
        {
            BrregApiBaseUrl = GetBrregUrlBasedOnEnvironment(webHostEnvironment),
            CacheOptions = new CacheOptions { Disabled = false },
        };

        configure?.Invoke(config);

        services.AddServices(webHostEnvironment, config);

        return services;
    }

    /// <summary>
    /// Registers an implementation of IEnhetsregisteret in the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="webHostEnvironment">the environment the application runs in</param>
    /// <param name="config">Configuration for the client. If null, a default configuration is used.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddEnhetsregisteret(
        this IServiceCollection services,
        IWebHostEnvironment webHostEnvironment,
        EnhetsregisteretConfig? config = null
    )
    {
        config ??= new EnhetsregisteretConfig()
        {
            BrregApiBaseUrl = GetBrregUrlBasedOnEnvironment(webHostEnvironment),
            CacheOptions = new CacheOptions { Disabled = false },
        };

        services.AddServices(webHostEnvironment, config);

        return services;
    }

    private static string GetBrregUrlBasedOnEnvironment(IWebHostEnvironment webHostEnvironment)
    {
        return webHostEnvironment.IsProduction()
            ? "https://data.brreg.no/"
            : "https://data.ppe.brreg.no/";
    }

    private static void AddServices(
        this IServiceCollection services,
        IWebHostEnvironment webHostEnvironment,
        EnhetsregisteretConfig config
    )
    {
        services.AddSingleton(config!);

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
}
