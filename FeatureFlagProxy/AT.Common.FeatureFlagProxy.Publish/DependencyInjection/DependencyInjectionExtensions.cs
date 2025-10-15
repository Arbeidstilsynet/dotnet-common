using Arbeidstilsynet.Common.FeatureFlagProxy.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Unleash;

namespace Arbeidstilsynet.Common.FeatureFlagProxy.DependencyInjection;

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registrerer en implementasjon av IFeatureFlagProxy med Unleash som backing service i den spesifiserte <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <param name="unleash">Unleash singleton instance som skal brukes for feature flag evaluering.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFeatureFlagProxy(this IServiceCollection services, IUnleash unleash)
    {
        services.AddSingleton(unleash);
        services.AddSingleton<IFeatureFlagProxy, FeatureFlagProxyImplementation>();

        return services;
    }
}
