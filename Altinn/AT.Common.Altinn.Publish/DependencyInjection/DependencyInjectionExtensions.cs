using Altinn.App.Core.Features;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation;
using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.AspNetCore.DependencyInjection;
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
    public required Uri AuthenticationUrl { get; init; }
    public required Uri StorageUrl { get; init; }
    public required Uri EventUrl { get; init; }

    public required Uri AppBaseUrl { get; init; }
}

public record AltinnAuthenticationConfiguration
{
    public required string CertificatePrivateKey { get; init; }
    public required string IntegrationId { get; init; }

    public required string[] Scopes { get; init; }

    public Uri? MaskinportenUrl { get; init; } = default;
}

/// <summary>
/// Dependency injection extensions for Altinn
/// </summary>
public static class DependencyInjectionExtensions
{
    internal const string AltinnOrgIdentifier = "dat";
    internal const string AltinnStorageApiClientKey = "AltinnStorageApiClient";
    internal const string AltinnAppApiClientKey = "AltinnEventsApiClient";

    internal const string AltinnAuthenticationApiClientKey = "AltinnAuthenticationApiClient";

    internal const string MaskinportenApiClientKey = "MaskinportenApiClient";

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
    /// Adds an adapter which contains convenience services for altinn communication. It also adds all available Altinn Clients to communicate with the Altinn 3 Apis.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="altinnTokenProvider">Implementation to retrieve a valid altinn token. If caching is required, it need to be implemented.</param>
    /// <param name="altinnApiConfiguration">Only required if it needs to be overwritten. By default, we determine BaseUrls based on the provided hostEnvironment.</param>
    /// <returns>Makes the usage of <see cref="IAltinnAdapter"/>, <see cref="IAltinnEventsClient"/> and <see cref="IAltinnStorageClient"/> available for the consumer.</returns>
    public static IServiceCollection AddAltinnAdapter(
        this IServiceCollection services,
        IWebHostEnvironment hostEnvironment,
        AltinnAuthenticationConfiguration altinnAuthenticationConfiguration,
        AltinnApiConfiguration? altinnApiConfiguration = null
    )
    {
        services.AddAltinnApiClients(
            hostEnvironment,
            altinnAuthenticationConfiguration,
            altinnApiConfiguration
        );
        services.AddScoped<IAltinnAdapter, AltinnAdapter>();
        services.AddScoped<IAltinnTokenAdapter, AltinnTokenAdapter>();

        return services;
    }

    /// <summary>
    /// Adds all available Altinn Clients to communicate with the Altinn 3 Apis.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="altinnAuthenticationConfiguration">Configuration for the altinn token exchange</param>
    /// <param name="altinnApiConfiguration">Only required if it needs to be overwritten. By default, we determine BaseUrls based on the provided hostEnvironment.</param>
    /// <returns>Makes the usage of <see cref="IAltinnEventsClient"/> and <see cref="IAltinnStorageClient"/> available for the consumer.</returns>
    public static IServiceCollection AddAltinnApiClients(
        this IServiceCollection services,
        IWebHostEnvironment hostEnvironment,
        AltinnAuthenticationConfiguration altinnAuthenticationConfiguration,
        AltinnApiConfiguration? altinnApiConfiguration = null
    )
    {
        altinnApiConfiguration ??= hostEnvironment.CreateDefaultAltinnApiConfiguration();
        services.AddSingleton(Options.Create(altinnApiConfiguration));
        services.AddSingleton(Options.Create(altinnAuthenticationConfiguration));
        services
            .AddHttpClient(
                AltinnAppApiClientKey,
                client =>
                {
                    client.BaseAddress = altinnApiConfiguration.EventUrl;
                }
            )
            .AddStandardResilienceHandler();
        services
            .AddHttpClient(
                AltinnStorageApiClientKey,
                client =>
                {
                    client.BaseAddress = altinnApiConfiguration.StorageUrl;
                }
            )
            .AddStandardResilienceHandler();
        services
            .AddHttpClient(
                AltinnAuthenticationApiClientKey,
                client =>
                {
                    client.BaseAddress = altinnApiConfiguration.AuthenticationUrl;
                }
            )
            .AddStandardResilienceHandler();
        services
            .AddMemoryCachedClient(
                MaskinportenApiClientKey,
                client =>
                {
                    client.BaseAddress =
                        altinnAuthenticationConfiguration.MaskinportenUrl
                        ?? new Uri(hostEnvironment.GetMaskinportenUrl());
                },
                options =>
                {
                    // max token lifetime is 120 seconds
                    options.AbsoluteExpiration = TimeSpan.FromSeconds(110);
                }
            )
            .AddStandardResilienceHandler();
        if (hostEnvironment.IsDevelopment())
        {
            services.AddSingleton<IAltinnTokenProvider, LocalAltinnTokenProvider>();
        }
        else
        {
            services.AddSingleton<IAltinnTokenProvider, AltinnTokenProvider>();
        }
        services.AddSingleton<IAltinnTokenAdapter, AltinnTokenAdapter>();
        services.AddTransient<IAltinnEventsClient, AltinnEventsClient>();
        services.AddTransient<IAltinnStorageClient, AltinnStorageClient>();
        services.AddTransient<IAltinnAuthenticationClient, AltinnAuthenticationClient>();
        services.AddTransient<IMaskinportenClient, MaskinportenClient>();
        return services;
    }
}
