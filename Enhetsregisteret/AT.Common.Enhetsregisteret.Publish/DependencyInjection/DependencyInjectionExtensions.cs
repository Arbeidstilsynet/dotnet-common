using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using Microsoft.Extensions.DependencyInjection;

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
        CacheOptions? cacheOptions = null
    )
    {
        BrregApiBaseUrl = brregApiBaseUrl ?? "https://data.brreg.no/";
        CacheOptions = new CacheOptions
        {
            Disabled = cacheOptions?.Disabled ?? false,
        };
    }

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
    /// <param name="configure">Configure the client.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddEnhetsregisteret(
        this IServiceCollection services,
        Action<EnhetsregisteretConfig>? configure = null
    )
    {
        var config = new EnhetsregisteretConfig();

        configure?.Invoke(config);

        services.AddServices(config);

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
        EnhetsregisteretConfig? config = null
    )
    {
        config ??= new EnhetsregisteretConfig();

        services.AddServices(config);

        return services;
    }

    private static void AddServices(this IServiceCollection services, EnhetsregisteretConfig config)
    {
        services
            .AddHttpClient(
                Clientkey,
                httpClient =>
                {
                    httpClient.BaseAddress = new Uri(config.BrregApiBaseUrl);
                }
            ).AddStandardResilienceHandler();

        services.AddSingleton(config!);
        services.AddMemoryCache();
        services.AddTransient<IEnhetsregisteret, EnhetsregisteretClient>();
    }
}
