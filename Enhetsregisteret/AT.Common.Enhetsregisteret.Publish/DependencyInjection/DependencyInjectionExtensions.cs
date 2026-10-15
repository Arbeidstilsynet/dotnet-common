using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Shared.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;

/// <summary>The implementation or API instance to use.</summary>
public enum EnhetsregisteretEnvironment
{
    /// <summary>The production Brreg API.</summary>
    Production,

    /// <summary>The Brreg pre-production API.</summary>
    Ppe,

    /// <summary>Offline convenience-interface stubs; the generated client is not registered.</summary>
    Stub,
}

/// <summary>
/// Configures the <see cref="HttpClient"/> for the Enhetsregisteret API.
/// </summary>
public class EnhetsregisteretConfig
{
    /// <summary>
    /// Defaults to Production in Production, Ppe in Staging, and Stub in Development.
    /// Required for other host environments. Production only permits the Production target.
    /// </summary>
    public EnhetsregisteretEnvironment? Environment { get; set; }

    /// <summary>
    /// Overrides the API base URL and selects the real adapter even when the target is Stub.
    /// If unset, the URL follows the resolved target. Non-default URLs are rejected in Production
    /// and logged at startup in Staging.
    /// </summary>
    public string? BrregApiBaseUrlOverwrite { get; set; }

    /// <summary>
    /// Settings for caching mechanism.
    /// </summary>
    public CacheOptions CacheOptions { get; set; } = new();
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
    /// <param name="hostEnvironment">the environment the application runs in</param>
    /// <param name="configure">Configure the client.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddEnhetsregisteret(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment,
        Action<EnhetsregisteretConfig> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        var config = new EnhetsregisteretConfig()
        {
            CacheOptions = new CacheOptions { Disabled = false },
        };

        configure.Invoke(config);

        services.AddServices(hostEnvironment, config);

        return services;
    }

    /// <summary>
    /// Registers an implementation of IEnhetsregisteret in the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="hostEnvironment">the environment the application runs in</param>
    /// <param name="config">Configuration for the client. If null, a default configuration is used.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddEnhetsregisteret(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment,
        EnhetsregisteretConfig? config = null
    )
    {
        config ??= new EnhetsregisteretConfig()
        {
            CacheOptions = new CacheOptions { Disabled = false },
        };

        services.AddServices(hostEnvironment, config);

        return services;
    }

    private static void AddServices(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment,
        EnhetsregisteretConfig config
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        var target = EnvironmentClientPolicy.ResolveTarget(
            hostEnvironment,
            config.Environment,
            EnhetsregisteretEnvironment.Production,
            EnhetsregisteretEnvironment.Ppe,
            EnhetsregisteretEnvironment.Stub
        );
        var defaultUrl =
            target == EnhetsregisteretEnvironment.Production
                ? "https://data.brreg.no/"
                : "https://data.ppe.brreg.no/";
        var baseUrl = EnvironmentClientPolicy.ResolveUrl(
            hostEnvironment,
            config.BrregApiBaseUrlOverwrite,
            defaultUrl
        );
        EnvironmentClientPolicy.WarnOnStagingOverrides(
            services,
            hostEnvironment,
            "Enhetsregisteret",
            (baseUrl, defaultUrl)
        );
        services.AddValidatorsFromAssemblyContaining<IAssemblyInfo>();
        services.AddSingleton(config);

        if (target == EnhetsregisteretEnvironment.Stub && config.BrregApiBaseUrlOverwrite is null)
        {
            services.AddScoped<Ports.IEnhetsregisteret, EnhetsregisteretStub>();
            return;
        }

        Action<HttpClient> configureClient = (httpClient) =>
        {
            httpClient.BaseAddress = baseUrl;
        };

        var clientBuilder = config.CacheOptions.Disabled
            ? services.AddHttpClient(Clientkey, configureClient)
            : services.AddMemoryCachedClient(Clientkey, configureClient);

        clientBuilder.AddStandardResilienceHandler();

        services.AddScoped<EnhetsregisteretRequestAdapter>();
        services.AddScoped<EnhetsregisteretClient>(serviceProvider => new EnhetsregisteretClient(
            serviceProvider.GetRequiredService<EnhetsregisteretRequestAdapter>()
        ));
        services.AddScoped<Ports.IEnhetsregisteret, EnhetsregisteretAdapter>();
    }
}
