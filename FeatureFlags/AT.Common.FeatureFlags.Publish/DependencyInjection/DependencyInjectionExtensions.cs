using Arbeidstilsynet.Common.FeatureFlags.Implementation;
using Arbeidstilsynet.Common.FeatureFlags.Ports;
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
    /// <param name="unleashSettings">Unleash settings to use for configuring the Unleash client.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFeatureFlags(this IServiceCollection services, UnleashSettings unleashSettings)
    {
        ArgumentNullException.ThrowIfNull(unleashSettings);


        services.AddSingleton<IUnleash>(provider =>
            new UnleashClientFactory().CreateClient(unleashSettings)
        );
        services.AddSingleton<IFeatureFlags, FeatureFlagsImplementation>();
        return services;

    }
}