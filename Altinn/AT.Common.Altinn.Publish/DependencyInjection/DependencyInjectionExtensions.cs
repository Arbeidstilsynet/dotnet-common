using Altinn.App.Core.Features;
using Arbeidstilsynet.Common.Altinn.Implementation;
using Arbeidstilsynet.Common.Altinn.Ports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.Altinn.DependencyInjection;

/// <summary>
/// Configuration for the LandOptions feature.
/// </summary>
/// <param name="OptionsId">The Altinn optionsId, default is "land".</param>
public record LandOptionsConfiguration(string OptionsId = "land");

/// <summary>
/// Configuration for Altinn 3 APIs.
/// </summary>
/// <param name="StorageUrl">Base Url for Altinn Storage Endpoints. See https://docs.altinn.studio/nb/api/storage/spec/</param>
/// <param name="EventUrl">Base Url for Altinn App Endpoints. See https://docs.altinn.studio/events/api/openapi/</param>
public record AltinnApiConfiguration
{
    public required string StorageUrl { get; init; }
    public required string EventUrl { get; init; }
}

/// <summary>
/// Dependency injection extensions for Altinn
/// </summary>
public static class DependencyInjectionExtensions
{
    private const string AltinnStorageApiBaseUrl = "https://platform.altinn.no/storage/api/v1";
    private const string AltinnStorageApiBaseUrlStaging = "https://platform.tt02.altinn.no/storage/api/v1";
    internal const string AltinnStorageApiClientKey = "AltinnStorageApiClient";
    private const string AltinnEventsApiBaseUrl = "https://platform.altinn.no/events/api/v1";
    private const string AltinnEventsApiBaseUrlStaging = "https://platform.tt02.altinn.no/events/api/v1";
    internal const string AltinnAppApiClientKey = "AltinnEventsApiClient";


    /// <summary>
    /// Adds a <see cref="ILandskodeLookup"/> to look up countries and their dial codes based on 3-letter ISO values.
    /// </summary>
    /// <param name="services"></param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddLandskoder(this IServiceCollection services)
    {
        services.TryAddSingleton<ILandskodeLookup, LandskodeLookup>();
        return services;
    }

    /// <summary>
    /// Adds the LandOptions feature to the service collection. This also adds <see cref="ILandskodeLookup"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="optionsConfiguration"></param>
    /// <returns></returns>
    public static IServiceCollection AddLandOptions(
        this IServiceCollection services,
        LandOptionsConfiguration? optionsConfiguration = null
    )
    {
        optionsConfiguration ??= new LandOptionsConfiguration();

        services.AddLandskoder();

        services.TryAddSingleton(Options.Create(optionsConfiguration));
        services.TryAddSingleton<IAppOptionsProvider, LandOptions>();

        return services;
    }

    /// <summary>
    /// Adds all available Altinn Clients to communicate with the Altinn 3 Apis.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="altinnTokenProvider">Implementation to retrieve a valid altinn token. If caching is required, it need to be implemented.</param>
    /// <param name="altinnApiConfiguration">Only required if it needs to be overwritten. By default, we determine BaseUrls based on the provided hostEnvironment.</param>
    /// <returns>Makes the usage of <see cref="IAltinnEventsClient"/> and <see cref="IAltinnStorageClient"/> available for the consumer.</returns>
    public static IServiceCollection AddAltinnApiClients(
        this IServiceCollection services,
        IWebHostEnvironment hostEnvironment,
        IAltinnTokenProvider altinnTokenProvider,
        AltinnApiConfiguration? altinnApiConfiguration = null
    )
    {
        altinnApiConfiguration ??= new AltinnApiConfiguration()
        {
            EventUrl = hostEnvironment.IsProduction() ? AltinnEventsApiBaseUrl : AltinnEventsApiBaseUrlStaging,
            StorageUrl = hostEnvironment.IsProduction() ? AltinnStorageApiBaseUrl : AltinnStorageApiBaseUrlStaging
        };
        services.AddHttpClient(AltinnAppApiClientKey,
                client => { client.BaseAddress = new Uri(altinnApiConfiguration.EventUrl); })
            .AddStandardResilienceHandler();
        services.AddHttpClient(AltinnStorageApiClientKey,
                client => { client.BaseAddress = new Uri(altinnApiConfiguration.StorageUrl); })
            .AddStandardResilienceHandler();
        services.AddSingleton(altinnTokenProvider);
        services.AddTransient<IAltinnEventsClient, AltinnEventsClient>();
        services.AddTransient<IAltinnStorageClient, AltinnStorageClient>();
        return services;
    }
}