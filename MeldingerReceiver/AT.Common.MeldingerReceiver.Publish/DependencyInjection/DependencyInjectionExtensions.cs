using Arbeidstilsynet.Common.MeldingerReceiver.Implementation;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Arbeidstilsynet.Common.MeldingerReceiver.DependencyInjection;

public record ValkeyConfiguration
{
    public required string ConnectionString { get; init; } = "localhost";
}

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registrerer en implementasjon av IMeldingerReceiver i den spesifiserte <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <param name="configuration"></param>
    /// <param name="meldingerConsumer"></param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddMeldingerReceiverWithBackgroundService(
        this IServiceCollection services,
        ValkeyConfiguration configuration,
        IMeldingerConsumer meldingerConsumer
    )
    {
        services.AddSingleton(meldingerConsumer);
        services.AddHostedService<ReceiverListener>();
        services.AddMeldingerReceiver(configuration);
        return services;
    }

    /// <summary>
    /// Registrerer en implementasjon av IMeldingerReceiver i den spesifiserte <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <param name="configuration"></param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddMeldingerReceiver(
        this IServiceCollection services,
        ValkeyConfiguration configuration
    )
    {
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(configuration.ConnectionString)
        );
        services.AddSingleton<IMeldingerReceiver, Implementation.MeldingerReceiver>();

        return services;
    }
}
