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
    /// Registers an implementation of <see cref="IFeatureFlags"/> in <paramref name="services"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to register the service in.</param>
    /// <param name="config">Feature flag settings.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFeatureFlags(
        this IServiceCollection services,
        FeatureFlagSettings config
    )
    {
        services.AddSingleton<IFeatureFlags, FeatureFlagsImplementation>();
        services.AddSingleton(config);
        if (string.IsNullOrEmpty(config.Url) || string.IsNullOrEmpty(config.ApiKey))
        {
            services.AddSingleton<IUnleash, FakeUnleash>();
        }
        else
        {
            var unleashSettings = new UnleashSettings
            {
                AppName = config.AppName,
                UnleashApi = new Uri(config.Url),
                CustomHttpHeaders = { { "Authorization", config.ApiKey } },
            };
            services.AddSingleton<IUnleash>(provider =>
                new UnleashClientFactory().CreateClient(unleashSettings)
            );
        }

        return services;
    }
}
