using System.Net.Http;
using Arbeidstilsynet.Common.Saksarkiv.DependencyInjection.Configuration;
using Arbeidstilsynet.Common.Saksarkiv.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace Arbeidstilsynet.Common.Saksarkiv.DependencyInjection;

/// <summary>
/// Extensions for dependency injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    internal const string SaksarkivHttpClientName = "SaksarkivHttpClient";

    /// <summary>
    /// Registers Saksarkiv client services with explicit configuration.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="configuration">Base client configuration.</param>
    /// <param name="configureResilience">
    /// Optional callback for customizing the standard HTTP resilience handler after the default Saksarkiv settings are applied.
    /// </param>
    public static IServiceCollection AddSaksarkivClient(
        this IServiceCollection services,
        SaksarkivConfiguration configuration,
        Action<HttpStandardResilienceOptions>? configureResilience = null
    )
    {
        services.AddSingleton(configuration);

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

        services.AddHealthChecks().AddCheck<SaksarkivHealthCheck>("Saksarkiv");
        services.AddScoped<SaksarkivAuthAdapter>();
        services.AddScoped<SaksarkivRequestAdapter>();
        services.AddScoped<SaksarkivClient>(serviceProvider => new SaksarkivClient(
            serviceProvider.GetRequiredService<SaksarkivRequestAdapter>()
        ));

        return services;
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

        return requestPath.StartsWith("/apiv2/health", StringComparison.OrdinalIgnoreCase);
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
