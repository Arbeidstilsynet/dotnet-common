using Arbeidstilsynet.Common.EraClient.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

namespace Arbeidstilsynet.Common.EraClient.Adapters.DependencyInjection;

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    internal const string RESILIENCE_PIPELINE_CONFIGKEY = "EraClientResiliencePipeline";
    internal const string AUTHCLIENT_KEY = "EraAuthenticationClient";

    internal const string ASBESTCLIENT_KEY = "EraAsbestClient";

    /// <summary>
    /// Registrerer en implementasjon av IEnhetsregisteret i den spesifiserte <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <param name="hostEnvironment">Host environment from the program starting the app. Hvis det ikke finnes noe BaseUrls i konfigurasjon, så bruker vi defaults basert på HostEnvironment.</param>
    /// <param name="configure">Konfigurer klienten.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddEraAdapter(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment,
        Action<EraClientConfiguration>? configure = null
    )
    {
        var config = new EraClientConfiguration();

        configure?.Invoke(config);

        services.AddServices(hostEnvironment, config);

        return services;
    }

    /// <summary>
    /// Registrerer en implementasjon av IEnhetsregisteret i den spesifiserte <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <param name="hostEnvironment">Host environment from the program starting the app. Hvis det ikke finnes noe BaseUrls i konfigurasjon, så bruker vi defaults basert på HostEnvironment.</param>
    /// <param name="config">Konfigurasjon for klienten. Hvis null, brukes en standardkonfigurasjon.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddEraAdapter(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment,
        EraClientConfiguration? config = null
    )
    {
        config ??= new EraClientConfiguration();

        services.AddServices(hostEnvironment, config);

        return services;
    }

    private static string GetAuthenticationUrl(
        IHostEnvironment hostEnvironment,
        EraClientConfiguration config
    )
    {
        if (config.AuthenticationUrl != null)
        {
            return config.AuthenticationUrl;
        }

        if (hostEnvironment.IsDevelopment())
        {
            return "https://dev-altinn-bemanning.auth.eu-west-1.amazoncognito.com/oauth2/token";
        }

        if (hostEnvironment.IsStaging())
        {
            return "https://test-altinn-bemanning.auth.eu-west-1.amazoncognito.com/oauth2/token";
        }

        return "https://prod-altinn-bemanning.auth.eu-west-1.amazoncognito.com/oauth2/token";
    }

    private static string GetAsbestUrl(
        IHostEnvironment hostEnvironment,
        EraClientConfiguration config
    )
    {
        if (config.EraAsbestUrl != null)
        {
            return config.EraAsbestUrl;
        }
        if (hostEnvironment.IsDevelopment())
        {
            return "https://data-verifi.arbeidstilsynet.no/asbest/api/virksomheter/";
        }

        if (hostEnvironment.IsStaging())
        {
            return "https://data-valid.arbeidstilsynet.no/asbest/api/virksomheter/";
        }

        return "https://data.arbeidstilsynet.no/asbest/api/virksomheter/";
    }

    private static void AddServices(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment,
        EraClientConfiguration config
    )
    {
        services
            .AddHttpClient(
                AUTHCLIENT_KEY,
                httpClient =>
                {
                    httpClient.BaseAddress = new Uri(GetAuthenticationUrl(hostEnvironment, config));
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
                    httpClient.BaseAddress = new Uri(GetAsbestUrl(hostEnvironment, config));
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
