using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Arbeidstilsynet.Common.GeoNorge.DependencyInjection;

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registrerer en implementasjon av IGeoNorge i den spesifiserte <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddGeoNorge(this IServiceCollection services)
    {
        services.AddSingleton<IGeoNorge, GeoNorgeImplementation>();

        return services;
    }
}
