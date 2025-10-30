using Arbeidstilsynet.Common.FeatureFlags.Implementation;
using Arbeidstilsynet.Common.FeatureFlags.Ports;
using Microsoft.Extensions.DependencyInjection;

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
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFeatureFlags(this IServiceCollection services)
    {
        services.AddSingleton<IFeatureFlags, FeatureFlagsImplementation>();

        return services;
    }
}
