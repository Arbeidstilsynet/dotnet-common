using Arbeidstilsynet.Common.BlubExtensions.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Arbeidstilsynet.Common.BlubExtensions.DependencyInjection;

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registrerer en implementasjon av IBlubExtensions i den spesifiserte <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddBlubExtensions(this IServiceCollection services)
    {
        services.AddSingleton<IBlubExtensions, BlubExtensionsImplementation>();

        return services;
    }
}
