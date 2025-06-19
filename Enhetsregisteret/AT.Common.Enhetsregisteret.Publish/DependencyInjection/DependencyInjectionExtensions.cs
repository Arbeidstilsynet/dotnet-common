using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Retry;

namespace Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;

/// <summary>
/// Konfigurerer HttpClient for Enhetsregisteret API.
/// </summary>
public class EnhetsregisteretConfig
{
    /// <summary>
    /// Initialiserer en ny instans av <see cref="EnhetsregisteretConfig"/> klassen.
    /// </summary>
    /// <param name="brregApiBaseUrl">Basis-URL for Enhetsregisteret API. Default: "https://data.brreg.no/enhetsregisteret/api/".</param>
    /// <param name="timeoutInSeconds">Maksimal ventetid for hver request i sekunder. Default: 30 sekunder.</param>
    /// <param name="retryStrategy">Retry-strategi for API-kall. Default retry-strategi:
    /// <br/>
    /// <br/>- Forsøker opptil 5 ganger med eksponentiell backoff og jitter.
    /// <br/>
    /// <br/>Denne strategien er designet for å håndtere midlertidige feil,
    /// og sikre mer pålitelige API-kall til Enhetsregisteret.
    /// </param>
    /// <param name="cacheOptions">Cache-instillinger for Enhetsregisteret.
    /// </param>
    public EnhetsregisteretConfig(
        string? brregApiBaseUrl = null,
        int timeoutInSeconds = 30,
        RetryStrategyOptions? retryStrategy = null,
        CacheOptions? cacheOptions = null
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeoutInSeconds, 0);

        BrregApiBaseUrl = brregApiBaseUrl ?? "https://data.brreg.no/";
        CacheOptions = new CacheOptions
        {
            Disabled = cacheOptions?.Disabled ?? false,
            ExpirationTime = cacheOptions?.ExpirationTime ?? TimeSpan.FromDays(1),
        };

        ResiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(
                retryStrategy
                    ?? new RetryStrategyOptions
                    {
                        ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                        Delay = TimeSpan.FromSeconds(2),
                        MaxRetryAttempts = 5,
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                    }
            )
            .AddTimeout(TimeSpan.FromSeconds(timeoutInSeconds))
            .Build();
    }

    /// <summary>
    /// Basis-URL for Enhetsregisteret API.
    /// </summary>
    public string BrregApiBaseUrl { get; set; }

    /// <summary>
    /// Settings for caching-mechanismen.
    /// </summary>
    public CacheOptions CacheOptions { get; set; }

    /// <summary>
    /// Resilience pipeline for API-kall.
    /// </summary>
    public ResiliencePipeline ResiliencePipeline { get; set; }
}

/// <summary>
/// Cache-instillinger for Enhetsregisteret.
/// </summary>
/// <param name="Disabled">Hvis true, inaktiveres cache-mekanismen. Default: false.</param>
/// <param name="ExpirationTime">Hvis cachen er aktivert, angir denne tiden hvor lenge cache-verdien skal beholdes. Default: 1 dag.</param>
public record CacheOptions(bool Disabled = false, TimeSpan? ExpirationTime = null);

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    internal const string RESILIENCE_PIPELINE_CONFIGKEY = "Enhetsregisteret";
    internal const string CLIENTKEY = "EnhetsregisteretClient";

    /// <summary>
    /// Registrerer en implementasjon av IEnhetsregisteret i den spesifiserte <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <param name="configure">Konfigurer klienten.</param>
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
    /// Registrerer en implementasjon av IEnhetsregisteret i den spesifiserte <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <param name="config">Konfigurasjon for klienten. Hvis null, brukes en standardkonfigurasjon.</param>
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
                CLIENTKEY,
                httpClient =>
                {
                    httpClient.BaseAddress = new Uri(config.BrregApiBaseUrl);
                }
            )
            .AddResilienceHandler(
                RESILIENCE_PIPELINE_CONFIGKEY,
                builder =>
                {
                    builder.AddPipeline(config.ResiliencePipeline);
                }
            );

        services.AddSingleton(config!);
        services.TryAddSingleton<IMemoryCache, MemoryCache>();
        services.AddTransient<IEnhetsregisteret, EnhetsregisteretClient>();
    }
}