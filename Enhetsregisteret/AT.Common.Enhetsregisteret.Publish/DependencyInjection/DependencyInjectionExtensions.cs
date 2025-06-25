using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Retry;

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
    /// <param name="timeoutInSeconds">Timeout for API requests. Default: 30 seconds.</param>
    /// <param name="retryStrategy">Retry strategy for API calls. Default retry strategy:
    /// <br/>
    /// <br/>- Tries up to 5 times with exponential backoff and jitter.
    /// <br/>
    /// <br/>This strategy is designed to handle temporary errors,
    /// and ensure more reliable API calls to Enhetsregisteret.
    /// </param>
    /// <param name="cacheOptions">Cache settings for Enhetsregisteret.
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
    /// BaseUrl for Enhetsregisteret API.
    /// </summary>
    public string BrregApiBaseUrl { get; set; }

    /// <summary>
    /// Settings for caching mechanism.
    /// </summary>
    public CacheOptions CacheOptions { get; set; }

    /// <summary>
    /// Resilience pipeline for API calls.
    /// </summary>
    public ResiliencePipeline ResiliencePipeline { get; set; }
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
    internal const string RESILIENCE_PIPELINE_CONFIGKEY = "Enhetsregisteret";
    internal const string CLIENTKEY = "EnhetsregisteretClient";

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
