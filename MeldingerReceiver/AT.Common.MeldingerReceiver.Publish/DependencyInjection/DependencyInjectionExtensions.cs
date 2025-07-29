using Arbeidstilsynet.Common.MeldingerReceiver.Implementation;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Arbeidstilsynet.Common.MeldingerReceiver.DependencyInjection;

public record ValkeyConfiguration
{
    public required string ConnectionString { get; init; } = "localhost";
}

public record MeldingerReceiverApiConfiguration
{
    public required string BaseUrl { get; init; } = "http://localhost:9008";
}

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    internal const string MeldingerReceiverApiClientKey = "MeldingerReceiverApiClient";

    /// <summary>
    /// Registrerer en implementasjon av IMeldingerReceiver i den spesifiserte <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <param name="valkeyConfiguration"></param>
    /// <param name="meldingerReceiverApiConfiguration"></param>
    /// <param name="meldingerConsumer"></param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddMeldingerReceiverWithBackgroundService(
        this IServiceCollection services,
        ValkeyConfiguration valkeyConfiguration,
        MeldingerReceiverApiConfiguration meldingerReceiverApiConfiguration,
        IMeldingerConsumer meldingerConsumer
    )
    {
        services.AddSingleton(meldingerConsumer);
        services.AddHostedService<ReceiverListener>();
        services.AddMeldingerReceiver(valkeyConfiguration, meldingerReceiverApiConfiguration);
        return services;
    }

    /// <summary>
    /// Registrerer en implementasjon av IMeldingerReceiver i den spesifiserte <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> som tjenesten skal legges til i.</param>
    /// <param name="valkeyConfiguration"></param>
    /// <param name="meldingerReceiverApiConfiguration"></param>
    /// <returns><see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddMeldingerReceiver(
        this IServiceCollection services,
        ValkeyConfiguration valkeyConfiguration,
        MeldingerReceiverApiConfiguration meldingerReceiverApiConfiguration
    )
    {
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(valkeyConfiguration.ConnectionString)
        );
        services.AddSingleton<IMeldingerReceiver, Implementation.MeldingerReceiver>();
        services
            .AddHttpClient(
                MeldingerReceiverApiClientKey,
                client =>
                {
                    client.BaseAddress = new Uri(meldingerReceiverApiConfiguration.BaseUrl);
                }
            )
            .AddStandardResilienceHandler();

        return services;
    }
}
