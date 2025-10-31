using Arbeidstilsynet.Common.FeatureFlags.Implementation;
using Arbeidstilsynet.Common.FeatureFlags.Model;
using Arbeidstilsynet.Common.FeatureFlags.Ports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Unleash;
using Unleash.ClientFactory;

namespace Arbeidstilsynet.Common.FeatureFlags.DependencyInjection;

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers an implementation av <see cref="IFeatureFlags"/> in <paramref name="services"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to register the service in.</param>
    /// <param name="webHostEnvironment">The web host environment.</param>
    /// <param name="config">Feature flag settings.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFeatureFlags(
        this IServiceCollection services,
        IWebHostEnvironment webHostEnvironment,
        FeatureFlagSettings? config
    )
    {
        services.AddSingleton<IFeatureFlags, FeatureFlagsImplementation>();

        if (
            config is null
            || string.IsNullOrWhiteSpace(config.Url)
            || string.IsNullOrWhiteSpace(config.ApiKey)
        )
        {
            services.AddSingleton<IUnleash, FakeUnleash>();
            return services;
        }

        var unleashSettings = new UnleashSettings
        {
            AppName = config.AppName,
            InstanceTag = webHostEnvironment.EnvironmentName,
            UnleashApi = new Uri(config.Url),
            CustomHttpHeaders = { { "Authorization", config.ApiKey } },
        };

        services.AddSingleton<IUnleash>(provider =>
            new UnleashClientFactory().CreateClient(unleashSettings)
        );
        return services;
    }
}
