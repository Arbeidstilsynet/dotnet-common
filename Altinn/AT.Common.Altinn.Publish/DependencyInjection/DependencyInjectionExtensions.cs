using System.ComponentModel.DataAnnotations;
using Altinn.App.Core.Features;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation;
using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Arbeidstilsynet.Common.AspNetCore.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
public record AltinnApiConfiguration
{
    /// <summary>
    /// The base URL for Altinn authentication endpoints. See https://docs.altinn.studio/nb/api/authentication/spec/
    /// </summary>
    public required Uri AuthenticationUrl { get; init; }

    /// <summary>
    /// The base URL for Altinn storage endpoints. See https://docs.altinn.studio/nb/api/storage/spec/
    /// </summary>
    public required Uri StorageUrl { get; init; }

    /// <summary>
    /// The base URL for Altinn event endpoints. See https://docs.altinn.studio/events/api/openapi/
    /// </summary>
    public required Uri EventUrl { get; init; }

    /// <summary>
    /// The base URL for the Altinn application.
    /// </summary>
    public required Uri AppBaseUrl { get; init; }
}

/// <summary>
/// Configuration for Altinn authentication.
/// </summary>
public record MaskinportenConfiguration
{
    /// <summary>
    /// The private key base64 encoded for the certificate used for authentication.
    /// </summary>
    [Required]
    [ConfigurationKeyName("")]
    public required string CertificatePrivateKey { get; init; }

    /// <summary>
    /// The certificate chain base64 encoded to be used as x5c header.
    /// </summary>
    [Required]
    [ConfigurationKeyName("CertificateChain")]
    public required string CertificateChain { get; init; }

    /// <summary>
    /// The integration ID for the Altinn application.
    /// </summary>
    [Required]
    [ConfigurationKeyName("IntegrationId")]
    public required string IntegrationId { get; init; }

    /// <summary>
    /// The scopes to request during authentication.
    /// </summary>
    [Required]
    [ConfigurationKeyName("Scopes")]
    public required string[] Scopes { get; init; }

    /// <summary>
    /// The base URL for Maskinporten authentication. Default will be determined based on the environment.
    /// </summary>
    public Uri? MaskinportenUrl { get; init; } = default;
}

/// <summary>
/// Dependency injection extensions for Altinn
/// </summary>
public static class DependencyInjectionExtensions
{
    internal const string AltinnOrgIdentifier = "dat";
    internal const string AltinnStorageApiClientKey = "AltinnStorageApiClient";
    internal const string AltinnEventsApiClientKey = "AltinnEventsApiClient";

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
    /// <param name="maskinportenConfiguration">Configuration for the altinn token exchange</param>
    /// <param name="altinnApiConfiguration">Only required if it needs to be overwritten. By default, we determine BaseUrls based on the provided hostEnvironment.</param>
    /// <returns>Makes the usage of <see cref="IAltinnAdapter"/>, <see cref="IAltinnEventsClient"/> and <see cref="IAltinnStorageClient"/> available for the consumer.</returns>
    public static IServiceCollection AddAltinnAdapter(
        this IServiceCollection services,
        IWebHostEnvironment hostEnvironment,
        MaskinportenConfiguration maskinportenConfiguration,
        AltinnApiConfiguration? altinnApiConfiguration = null
    )
    {
        services.AddAltinnApiClients(
            hostEnvironment,
            maskinportenConfiguration,
            altinnApiConfiguration
        );
        services.AddScoped<IAltinnAdapter, AltinnAdapter>();

        return services;
    }

    /// <summary>
    /// Adds all available Altinn Clients to communicate with the Altinn 3 Apis.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="maskinportenConfiguration">Configuration for the altinn token exchange</param>
    /// <param name="altinnApiConfiguration">Only required if it needs to be overwritten. By default, we determine BaseUrls based on the provided hostEnvironment.</param>
    /// <returns>Makes the usage of <see cref="IAltinnEventsClient"/> and <see cref="IAltinnStorageClient"/> available for the consumer.</returns>
    public static IServiceCollection AddAltinnApiClients(
        this IServiceCollection services,
        IWebHostEnvironment hostEnvironment,
        MaskinportenConfiguration maskinportenConfiguration,
        AltinnApiConfiguration? altinnApiConfiguration = null
    )
    {
        altinnApiConfiguration ??= hostEnvironment.CreateDefaultAltinnApiConfiguration();
        services.AddSingleton(Options.Create(altinnApiConfiguration));
        services.AddSingleton(Options.Create(maskinportenConfiguration));
        if (hostEnvironment.IsDevelopment())
        {
            services.AddSingleton<IAltinnTokenProvider, LocalAltinnTokenProvider>();
        }
        else
        {
            services.AddSingleton<IAltinnTokenProvider, AltinnTokenProvider>();
        }

        return services.AddAltinnApiClientsInternal(
            hostEnvironment,
            maskinportenConfiguration,
            altinnApiConfiguration
        );
    }

    private static IServiceCollection AddAltinnApiClientsInternal(
        this IServiceCollection services,
        IWebHostEnvironment hostEnvironment,
        MaskinportenConfiguration maskinportenConfiguration,
        AltinnApiConfiguration altinnApiConfiguration
    )
    {
        services
            .AddHttpClient(
                AltinnEventsApiClientKey,
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
            .AddHttpClient(
                MaskinportenApiClientKey,
                client =>
                {
                    client.BaseAddress =
                        maskinportenConfiguration.MaskinportenUrl
                        ?? new Uri(hostEnvironment.GetMaskinportenUrl());
                }
            )
            .AddStandardResilienceHandler();

        services.AddTransient<IAltinnEventsClient, AltinnEventsClient>();
        services.AddTransient<IAltinnStorageClient, AltinnStorageClient>();
        services.AddTransient<IAltinnAuthenticationClient, AltinnAuthenticationClient>();
        services.AddTransient<IMaskinportenClient, MaskinportenClient>();

        return services;
    }
}
