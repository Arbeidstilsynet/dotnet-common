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
        var settings = config ?? new FeatureFlagSettings();
        services.AddSingleton(settings);
        if (string.IsNullOrWhiteSpace(settings.Url) || string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            services.AddSingleton<IUnleash, FakeUnleash>();
        }
        else
        {
            var unleashSettings = new UnleashSettings
            {
                AppName = settings.AppName,
                InstanceTag = webHostEnvironment.EnvironmentName,
                UnleashApi = new Uri(settings.Url),
                CustomHttpHeaders = { { "Authorization", settings.ApiKey } },
            };
            services.AddSingleton<IUnleash>(provider =>
                new UnleashClientFactory().CreateClient(unleashSettings)
            );
        }

        return services;
    }
}
