using Arbeidstilsynet.Common.MeldingerReceiver.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Arbeidstilsynet.Common.MeldingerReceiver.DependencyInjection;

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registrerer en implementasjon av IMeldingerReceiver i den spesifiserte <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddMeldingerReceiver(this IServiceCollection services)
    {
        services.AddSingleton<IMeldingerReceiver, MeldingerReceiverImplementation>();

        return services;
    }
}
