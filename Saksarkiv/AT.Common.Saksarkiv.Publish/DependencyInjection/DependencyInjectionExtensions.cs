using System.Net.Http;
using Arbeidstilsynet.Common.Saksarkiv.DependencyInjection.Configuration;
using Arbeidstilsynet.Common.Saksarkiv.Implementation;
using Arbeidstilsynet.Common.Saksarkiv.V3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;

namespace Arbeidstilsynet.Common.Saksarkiv.DependencyInjection;

/// <summary>
/// Extensions for dependency injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    internal const string SaksarkivHttpClientName = "SaksarkivHttpClient";
    internal const string HealthProbePath = "/api/health";

    /// <summary>
    /// Registers the Saksarkiv <b>v3</b> client (the default) with explicit configuration.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="configuration">Base client configuration.</param>
    /// <param name="configureResilience">
    /// Optional callback for customizing the standard HTTP resilience handler after the default Saksarkiv settings are applied.
    /// </param>
    /// <remarks>
    /// Resolve <see cref="SaksarkivClientV3"/> to call the v3 API. To additionally use the legacy
    /// v2 API, call <see cref="AddSaksarkivClientV2"/>. This registration also adds a health check
    /// named <c>Saksarkiv</c> that probes the version-agnostic <c>GET /api/health/authPing</c>
    /// endpoint.
    /// </remarks>
    public static IServiceCollection AddSaksarkivClient(
        this IServiceCollection services,
        SaksarkivConfiguration configuration,
        Action<HttpStandardResilienceOptions>? configureResilience = null
    )
    {
        AddSharedInfrastructure(services, configuration, configureResilience);

        services.AddScoped<SaksarkivClientV3>(serviceProvider => new SaksarkivClientV3(
            serviceProvider.GetRequiredService<SaksarkivRequestAdapter>()
        ));

        services.TryAddScoped<ISaksarkivHealthPinger, SaksarkivHealthPinger>();
        services.AddHealthChecks().AddCheck<SaksarkivHealthCheck>("Saksarkiv");

        return services;
    }

    /// <summary>
    /// Registers the legacy Saksarkiv <b>v2</b> client on demand.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="configuration">Base client configuration.</param>
    /// <param name="configureResilience">
    /// Optional callback for customizing the standard HTTP resilience handler after the default Saksarkiv settings are applied.
    /// </param>
    /// <remarks>
    /// Resolve <see cref="SaksarkivClient"/> to call the v2 API. The v2 registration does not add a
    /// health check; the <c>Saksarkiv</c> health check is only registered by
    /// <see cref="AddSaksarkivClient"/> (v3).
    /// </remarks>
    public static IServiceCollection AddSaksarkivClientV2(
        this IServiceCollection services,
        SaksarkivConfiguration configuration,
        Action<HttpStandardResilienceOptions>? configureResilience = null
    )
    {
        AddSharedInfrastructure(services, configuration, configureResilience);

        services.AddScoped<SaksarkivClient>(serviceProvider => new SaksarkivClient(
            serviceProvider.GetRequiredService<SaksarkivRequestAdapter>()
        ));

        return services;
    }

    private static void AddSharedInfrastructure(
        IServiceCollection services,
        SaksarkivConfiguration configuration,
        Action<HttpStandardResilienceOptions>? configureResilience
    )
    {
        services.TryAddSingleton(configuration);

        services
            .AddHttpClient(
                SaksarkivHttpClientName,
                client =>
                {
                    client.BaseAddress = new Uri(configuration.BaseUrl);
                }
            )
            .AddStandardResilienceHandler(options =>
            {
                ConfigureDefaultResilience(options);
                configureResilience?.Invoke(options);
            });

        services.TryAddScoped<SaksarkivAuthAdapter>();
        services.TryAddScoped<SaksarkivRequestAdapter>();
    }

    internal static bool ShouldSkipRetryForRequest(HttpMethod? method, string? requestPath)
    {
        if (method == HttpMethod.Post)
        {
            return true;
        }

        if (requestPath is null)
        {
            return false;
        }

        return requestPath.StartsWith(HealthProbePath, StringComparison.OrdinalIgnoreCase);
    }

    private static void ConfigureDefaultResilience(HttpStandardResilienceOptions options)
    {
        options.Retry.MaxRetryAttempts = 2;
        options.Retry.Delay = TimeSpan.FromSeconds(5);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(1);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15);
        options.CircuitBreaker.SamplingDuration =
            options.AttemptTimeout.Timeout * 2 + TimeSpan.FromSeconds(1);

        var defaultShouldHandle = options.Retry.ShouldHandle;
        options.Retry.ShouldHandle = args =>
            ShouldSkipRetryForRequest(
                args.Outcome.Result?.RequestMessage?.Method,
                args.Outcome.Result?.RequestMessage?.RequestUri?.AbsolutePath
            )
                ? ValueTask.FromResult(false)
                : defaultShouldHandle(args);
    }
}
