using Arbeidstilsynet.Common.EraClient;
using Arbeidstilsynet.Common.EraClient.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;

namespace Arbeidstilsynet.Common.EraClient.DependencyInjection;

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    internal const string RESILIENCE_PIPELINE_CONFIGKEY = "EraClientResiliencePipeline";
    internal const string AUTHCLIENT_KEY = "EraAuthenticationClient";

    internal const string ASBESTCLIENT_KEY = "EraAsbestClient";

    /// <summary>
    /// Register all EraClients for the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to be extended.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddEraAdapter(this IServiceCollection services)
    {
        services.AddServices(new EraClientConfiguration());
        return services;
    }

    /// <summary>
    /// Register all EraClients for the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to be extended.</param>
    /// <param name="configure">Configure action for the appropriate Configuration.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddEraAdapter(
        this IServiceCollection services,
        Action<EraClientConfiguration>? configure
    )
    {
        var config = new EraClientConfiguration();

        configure?.Invoke(config);

        services.AddServices(config);

        return services;
    }

    /// <summary>
    /// Register all EraClients for the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to be extended.</param>
    /// <param name="config">Config for all clients. If null, the default configuration is used.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddEraAdapter(
        this IServiceCollection services,
        EraClientConfiguration? config
    )
    {
        config ??= new EraClientConfiguration();

        services.AddServices(config);

        return services;
    }

    private static void AddServices(this IServiceCollection services, EraClientConfiguration config)
    {
        services
            .AddHttpClient(
                AUTHCLIENT_KEY,
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", AUTHCLIENT_KEY);
                }
            )
            .AddResilienceHandler(
                RESILIENCE_PIPELINE_CONFIGKEY,
                builder =>
                {
                    builder.AddPipeline(config.ResiliencePipeline);
                }
            );

        services
            .AddHttpClient(
                ASBESTCLIENT_KEY,
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", ASBESTCLIENT_KEY);
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
        services.AddTransient<IAuthenticationClient, AuthenticationClient>();
        services.AddTransient<IEraAsbestClient, EraAsbestClient>();
    }
}
