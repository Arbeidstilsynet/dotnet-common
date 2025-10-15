using Arbeidstilsynet.Common.FeatureFlagProxy.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Unleash;
using Unleash.ClientFactory;

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
    /// <param name="unleash">Unleash singleton instance som skal brukes for feature flag evaluering. Må håndtere disposal selv.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFeatureFlagProxy(this IServiceCollection services, IUnleash unleash)
    {
        ArgumentNullException.ThrowIfNull(unleash);

        services.AddSingleton(unleash);
        services.AddSingleton<IFeatureFlagProxy, FeatureFlagProxyImplementation>();

        return services;
    }

    /// <summary>
    /// Registrerer en implementasjon av IFeatureFlagProxy med Unleash som backing service i den spesifiserte <see cref="IServiceCollection"/>.
    /// Oppretter Unleash-klient ved hjelp av modern ClientFactory basert på de oppgitte innstillingene. Unleash-instansen blir automatisk disposed når DI-containeren blir disposed.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <param name="unleashSettings">Unleash-innstillinger som skal brukes for å konfigurere Unleash-klienten.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFeatureFlagProxy(this IServiceCollection services, UnleashSettings unleashSettings)
    {
        ArgumentNullException.ThrowIfNull(unleashSettings);

        // Register Unleash as singleton using modern ClientFactory (recommended since v1.5.0)
        services.AddSingleton<IUnleash>(provider => new UnleashClientFactory().CreateClient(unleashSettings));
        services.AddSingleton<IFeatureFlagProxy, FeatureFlagProxyImplementation>();

        return services;
    }

    /// <summary>
    /// Registrerer en implementasjon av IFeatureFlagProxy med Unleash som backing service i den spesifiserte <see cref="IServiceCollection"/> asynkront.
    /// Oppretter Unleash-klient ved hjelp av modern ClientFactory.CreateClientAsync basert på de oppgitte innstillingene. Unleash-instansen blir automatisk disposed når DI-containeren blir disposed.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <param name="unleashSettings">Unleash-innstillinger som skal brukes for å konfigurere Unleash-klienten.</param>
    /// <returns>Task som representerer den asynkrone operasjonen som returnerer <see cref="IServiceCollection"/> for chaining.</returns>
    public static async Task<IServiceCollection> AddFeatureFlagProxyAsync(this IServiceCollection services, UnleashSettings unleashSettings)
    {
        ArgumentNullException.ThrowIfNull(unleashSettings);

        // Create Unleash client asynchronously using modern ClientFactory (recommended since v1.5.0)
        var unleashClient = await new UnleashClientFactory().CreateClientAsync(unleashSettings);

        services.AddSingleton<IUnleash>(unleashClient);
        services.AddSingleton<IFeatureFlagProxy, FeatureFlagProxyImplementation>();

        return services;
    }
}
