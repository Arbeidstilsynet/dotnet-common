using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Implementation.Configuration;
using Arbeidstilsynet.Common.Altinn.Implementation.Token;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.Altinn.DependencyInjection;

/// <summary>
/// Dependency injection extensions for Altinn
/// </summary>
public static class DependencyInjectionExtensions
{
    internal const string AltinnStorageApiClientKey = "AltinnStorageApiClient";
    internal const string AltinnEventsApiClientKey = "AltinnEventsApiClient";
    internal const string AltinnAppsApiClientKey = "AltinnAppsApiClient";
    internal const string AltinnCorrespondenceApiClientKey = "AltinnCorrespondenceApiClient";
    internal const string AltinnAuthenticationApiClientKey = "AltinnAuthenticationApiClient";
    internal const string DialogportenApiClientKey = "DialogportenApiClient";
    internal const string MaskinportenApiClientKey = "MaskinportenApiClient";

    /// <summary>
    /// Adds an adapter which contains convenience services for altinn communication. It also adds all available Altinn Clients to communicate with the Altinn 3 Apis.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="maskinportenConfiguration">Configuration for the altinn token exchange</param>
    /// <param name="altinnConfiguration">
    /// Determines which Altinn instance to target. Required unless the host environment is Production or Staging.
    /// See <see cref="AltinnConfiguration.Environment"/>.
    /// </param>
    /// <returns>Makes the usage of <see cref="IAltinnAdapter"/>, <see cref="IAltinnEventsClient"/> and <see cref="IAltinnStorageClient"/> available for the consumer.</returns>
    /// <exception cref="InvalidOperationException">
    /// If the host environment is neither Production nor Staging and no <see cref="AltinnConfiguration.Environment"/>
    /// was supplied, or if base URLs are overridden while running in Production.
    /// </exception>
    public static IServiceCollection AddAltinnAdapter(
        this IServiceCollection services,
        IWebHostEnvironment hostEnvironment,
        MaskinportenConfiguration maskinportenConfiguration,
        AltinnConfiguration? altinnConfiguration = null
    )
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        services.AddAltinnApiClients(
            hostEnvironment,
            maskinportenConfiguration,
            altinnConfiguration
        );
        services.AddScoped<IAltinnAdapter, AltinnAdapter>();
        services.AddScoped<IAltinnMeldingerAdapter, AltinnMeldingerAdapter>();

        return services;
    }

    /// <summary>
    /// Adds all available Altinn Clients to communicate with the Altinn 3 Apis.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="maskinportenConfiguration">Configuration for the altinn token exchange</param>
    /// <param name="altinnConfiguration">
    /// Determines which Altinn instance to target. Required unless the host environment is Production or Staging.
    /// See <see cref="AltinnConfiguration.Environment"/>.
    /// </param>
    /// <returns>Makes the usage of <see cref="IAltinnEventsClient"/> and <see cref="IAltinnStorageClient"/> available for the consumer.</returns>
    /// <exception cref="InvalidOperationException">
    /// If the host environment is neither Production nor Staging and no <see cref="AltinnConfiguration.Environment"/>
    /// was supplied, or if base URLs are overridden while running in Production.
    /// </exception>
    public static IServiceCollection AddAltinnApiClients(
        this IServiceCollection services,
        IWebHostEnvironment hostEnvironment,
        MaskinportenConfiguration maskinportenConfiguration,
        AltinnConfiguration? altinnConfiguration = null
    )
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        altinnConfiguration ??= new AltinnConfiguration();

        var resolution = AltinnUrlResolver.Resolve(hostEnvironment, altinnConfiguration);

        services.AddSingleton(Options.Create(altinnConfiguration));
        services.AddSingleton(Options.Create(maskinportenConfiguration));
        services.AddSingleton(resolution);
        services.AddSingleton(resolution.Urls);

        if (resolution.EffectiveOverrides.Count > 0)
        {
            services.AddHostedService<AltinnUrlOverrideWarningService>();
        }

        // The token provider follows the resolved Altinn target rather than the host environment:
        // the local test-token endpoint only issues tokens a local Altinn instance accepts.
        if (resolution.Target == AltinnEnvironment.Local)
        {
            services.AddSingleton<IAltinnTokenProvider, LocalAltinnTokenProvider>();
        }
        else
        {
            services.AddSingleton<IAltinnTokenProvider, AltinnTokenProvider>();
        }

        return services.AddAltinnApiClientsInternal(resolution.Urls);
    }

    private static IServiceCollection AddAltinnApiClientsInternal(
        this IServiceCollection services,
        ResolvedAltinnUrls urls
    )
    {
        services.AddAltinnHttpClient(AltinnAppsApiClientKey, urls.AppBaseUrl);
        services.AddAltinnHttpClient(AltinnEventsApiClientKey, urls.EventsUrl);
        services.AddAltinnHttpClient(AltinnStorageApiClientKey, urls.StorageUrl);
        services.AddAltinnHttpClient(AltinnCorrespondenceApiClientKey, urls.CorrespondenceUrl);
        services.AddAltinnHttpClient(AltinnAuthenticationApiClientKey, urls.AuthenticationUrl);
        services.AddAltinnHttpClient(DialogportenApiClientKey, urls.DialogportenUrl);
        services.AddAltinnHttpClient(MaskinportenApiClientKey, urls.MaskinportenUrl);

        services.AddTransient<IAltinnAppsClient, AltinnAppsClient>();
        services.AddTransient<IAltinnEventsClient, AltinnEventsClient>();
        services.AddTransient<IAltinnStorageClient, AltinnStorageClient>();
        services.AddTransient<IAltinnAuthenticationClient, AltinnAuthenticationClient>();
        services.AddTransient<IAltinnCorrespondenceClient, AltinnCorrespondenceClient>();
        services.AddTransient<IAltinnDialogportenClient, AltinnDialogportenClient>();
        services.AddTransient<IMaskinportenClient, MaskinportenClient>();

        return services;
    }

    private static void AddAltinnHttpClient(
        this IServiceCollection services,
        string name,
        Uri baseAddress
    )
    {
        services
            .AddHttpClient(name, client => client.BaseAddress = baseAddress)
            .AddStandardResilienceHandler();
    }
}
