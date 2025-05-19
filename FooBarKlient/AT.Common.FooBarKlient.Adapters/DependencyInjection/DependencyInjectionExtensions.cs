using Arbeidstilsynet.Common.FooBarKlient;
using Arbeidstilsynet.Common.FooBarKlient.Adapters;
using Arbeidstilsynet.Common.FooBarKlient.Ports;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Retry;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registrerer en implementasjon av IFooBarKlient i den spesifiserte <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFooBarKlient(this IServiceCollection services)
    {
        services.AddSingleton<IFooBarKlient, FooBarKlientImplementation>();

        return services;
    }
}
