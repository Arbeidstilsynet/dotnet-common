using Arbeidstilsynet.Common.Altinn.Apps;
using Arbeidstilsynet.Common.Altinn.Authentication;
using Arbeidstilsynet.Common.Altinn.Correspondence;
using Arbeidstilsynet.Common.Altinn.Dialogporten;
using Arbeidstilsynet.Common.Altinn.Events;
using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Implementation.Configuration;
using Arbeidstilsynet.Common.Altinn.Implementation.Token;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Arbeidstilsynet.Common.Altinn.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions;

namespace Arbeidstilsynet.Common.Altinn.DependencyInjection;

/// <summary>
/// Dependency injection extensions for Altinn.
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
    /// Configures Altinn access and returns a builder for adding the clients the application needs.
    /// </summary>
    /// <remarks>
    /// This registers only the shared plumbing -- configuration, URL resolution and the token
    /// pipeline. Add the APIs you use on the returned builder:
    /// <code>
    /// services
    ///     .AddAltinn(builder.Environment, appSettings.MaskinportenConfiguration)
    ///     .AddStorage(o => o.Scopes = ["altinn:serviceowner/instances.read"])
    ///     .AddAltinnAdapter();
    /// </code>
    /// </remarks>
    /// <param name="services"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="maskinportenConfiguration">
    /// Credentials for the Maskinporten token exchange. Its <see cref="MaskinportenConfiguration.Scopes"/>
    /// are the default for every client that does not state its own.
    /// </param>
    /// <param name="altinnConfiguration">
    /// Determines which Altinn instance to target. Required unless the host environment is Production or Staging.
    /// See <see cref="AltinnConfiguration.Environment"/>.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// If the host environment is neither Production nor Staging and no <see cref="AltinnConfiguration.Environment"/>
    /// was supplied, or if base URLs are overridden while running in Production.
    /// </exception>
    public static IAltinnBuilder AddAltinn(
        this IServiceCollection services,
        IWebHostEnvironment hostEnvironment,
        MaskinportenConfiguration maskinportenConfiguration,
        AltinnConfiguration? altinnConfiguration = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(hostEnvironment);
        ArgumentNullException.ThrowIfNull(maskinportenConfiguration);

        altinnConfiguration ??= new AltinnConfiguration();

        var resolution = AltinnUrlResolver.Resolve(hostEnvironment, altinnConfiguration);
        var overrides = new AltinnOverrideRegistry(resolution.EffectiveOverrides);

        services.AddSingleton(Options.Create(altinnConfiguration));
        services.AddSingleton(Options.Create(maskinportenConfiguration));
        services.AddSingleton(resolution);
        services.AddSingleton(resolution.Urls);
        services.AddSingleton(overrides);

        // Every client's options are given their defaults here, whether or not that client is
        // registered, so an effective base URL and scope set can always be read back.
        foreach (var (clientName, defaultUrl) in DefaultUrls(resolution.Urls))
        {
            services.PostConfigure<AltinnClientOptions>(
                clientName,
                options =>
                {
                    options.BaseUrl ??= defaultUrl;

                    // An empty scope set is as unusable as an absent one, so both fall back.
                    if (options.Scopes is null or { Length: 0 })
                    {
                        options.Scopes = maskinportenConfiguration.Scopes;
                    }
                }
            );
        }

        // The token provider follows the resolved Altinn target rather than the host environment:
        // the local test-token endpoint only issues tokens a local Altinn instance accepts.
        if (resolution.Target == AltinnEnvironment.Local)
        {
            services.TryAddSingleton<IAltinnTokenProvider, LocalAltinnTokenProvider>();
        }
        else
        {
            services.TryAddSingleton<IAltinnTokenProvider, AltinnTokenProvider>();
        }

        var builder = new AltinnBuilder(services, resolution, overrides, hostEnvironment);

        builder.AddAuthenticationInfrastructure();

        if (resolution.EffectiveOverrides.Count > 0)
        {
            builder.EnsureOverrideWarningRegistered();
        }

        return builder;
    }

    /// <summary>
    /// Adds the Altinn storage API client, exposing <see cref="IAltinnStorageClient"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure">Overrides the base URL and/or scopes for this API.</param>
    public static IAltinnBuilder AddStorage(
        this IAltinnBuilder builder,
        Action<AltinnClientOptions>? configure = null
    )
    {
        var altinn = builder.Configure(AltinnClients.Storage, configure);

        if (altinn.TryRegister(AltinnClients.Storage))
        {
            altinn.RequireScopes(AltinnClients.Storage);
            altinn.AddAltinnHttpClient(AltinnStorageApiClientKey, AltinnClients.Storage);
            altinn.AddGeneratedClient<StorageRequestAdapter, StorageApiClient>(
                AltinnClients.Storage,
                adapter => new StorageApiClient(adapter)
            );
            altinn.Services.AddTransient<IAltinnStorageClient, AltinnStorageClient>();
        }

        return builder;
    }

    /// <summary>
    /// Adds the Altinn events API client, exposing <see cref="IAltinnEventsClient"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure">Overrides the base URL and/or scopes for this API.</param>
    public static IAltinnBuilder AddEvents(
        this IAltinnBuilder builder,
        Action<AltinnClientOptions>? configure = null
    )
    {
        var altinn = builder.Configure(AltinnClients.Events, configure);

        if (altinn.TryRegister(AltinnClients.Events))
        {
            altinn.RequireScopes(AltinnClients.Events);
            altinn.AddAltinnHttpClient(AltinnEventsApiClientKey, AltinnClients.Events);
            altinn.AddGeneratedClient<EventsRequestAdapter, EventsApiClient>(
                AltinnClients.Events,
                adapter => new EventsApiClient(adapter)
            );
            altinn.Services.AddTransient<IAltinnEventsClient, AltinnEventsClient>();
        }

        return builder;
    }

    /// <summary>
    /// Adds the Altinn apps API client, exposing <see cref="IAltinnAppsClient"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure">Overrides the base URL and/or scopes for this API.</param>
    public static IAltinnBuilder AddApps(
        this IAltinnBuilder builder,
        Action<AltinnClientOptions>? configure = null
    )
    {
        var altinn = builder.Configure(AltinnClients.Apps, configure);

        if (altinn.TryRegister(AltinnClients.Apps))
        {
            altinn.RequireScopes(AltinnClients.Apps);
            altinn.AddAltinnHttpClient(AltinnAppsApiClientKey, AltinnClients.Apps);
            altinn.AddGeneratedClient<AppsRequestAdapter, AppsApiClient>(
                AltinnClients.Apps,
                adapter => new AppsApiClient(adapter)
            );
            altinn.Services.AddTransient<IAltinnAppsClient, AltinnAppsClient>();
        }

        return builder;
    }

    /// <summary>
    /// Adds the Altinn correspondence API client, exposing <see cref="IAltinnCorrespondenceClient"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure">Overrides the base URL and/or scopes for this API.</param>
    public static IAltinnBuilder AddCorrespondence(
        this IAltinnBuilder builder,
        Action<AltinnClientOptions>? configure = null
    )
    {
        var altinn = builder.Configure(AltinnClients.Correspondence, configure);

        if (altinn.TryRegister(AltinnClients.Correspondence))
        {
            altinn.RequireScopes(AltinnClients.Correspondence);
            altinn.AddAltinnHttpClient(
                AltinnCorrespondenceApiClientKey,
                AltinnClients.Correspondence
            );
            altinn.AddGeneratedClient<CorrespondenceRequestAdapter, CorrespondenceApiClient>(
                AltinnClients.Correspondence,
                adapter => new CorrespondenceApiClient(adapter)
            );
            altinn.Services.AddTransient<IAltinnCorrespondenceClient, AltinnCorrespondenceClient>();
        }

        return builder;
    }

    /// <summary>
    /// Adds the Dialogporten API client, exposing <see cref="IAltinnDialogportenClient"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure">Overrides the base URL and/or scopes for this API.</param>
    public static IAltinnBuilder AddDialogporten(
        this IAltinnBuilder builder,
        Action<AltinnClientOptions>? configure = null
    )
    {
        var altinn = builder.Configure(AltinnClients.Dialogporten, configure);

        if (altinn.TryRegister(AltinnClients.Dialogporten))
        {
            altinn.RequireScopes(AltinnClients.Dialogporten);
            altinn.AddAltinnHttpClient(DialogportenApiClientKey, AltinnClients.Dialogporten);
            altinn.AddGeneratedClient<DialogportenRequestAdapter, DialogportenApiClient>(
                AltinnClients.Dialogporten,
                adapter => new DialogportenApiClient(adapter)
            );
            altinn.Services.AddTransient<IAltinnDialogportenClient, AltinnDialogportenClient>();
        }

        return builder;
    }

    /// <summary>
    /// Adds <see cref="IAltinnAdapter"/>, which provides convenience operations over instances and
    /// event subscriptions.
    /// </summary>
    /// <remarks>
    /// This depends on the storage and events APIs, so both are added if they have not been already.
    /// Configure them by calling <see cref="AddStorage"/> and <see cref="AddEvents"/> as well --
    /// order does not matter.
    /// </remarks>
    public static IAltinnBuilder AddAltinnAdapter(this IAltinnBuilder builder)
    {
        builder.AddStorage().AddEvents();

        builder.AsAltinnBuilder().Services.TryAddScoped<IAltinnAdapter, AltinnAdapter>();

        return builder;
    }

    /// <summary>
    /// Adds <see cref="IAltinnMeldingerAdapter"/>, which provides convenience operations over
    /// correspondence.
    /// </summary>
    /// <remarks>
    /// This depends on the correspondence API, so it is added if it has not been already. Configure
    /// it by calling <see cref="AddCorrespondence"/> as well -- order does not matter.
    /// </remarks>
    public static IAltinnBuilder AddMeldingerAdapter(this IAltinnBuilder builder)
    {
        builder.AddCorrespondence();

        builder
            .AsAltinnBuilder()
            .Services.TryAddScoped<IAltinnMeldingerAdapter, AltinnMeldingerAdapter>();

        return builder;
    }

    /// <summary>
    /// Adds every Altinn API client the package offers.
    /// </summary>
    /// <remarks>
    /// Prefer adding only the APIs you use, so each token carries only the scopes it needs.
    /// </remarks>
    public static IAltinnBuilder AddAllClients(this IAltinnBuilder builder)
    {
        return builder.AddStorage().AddEvents().AddApps().AddCorrespondence().AddDialogporten();
    }

    /// <summary>
    /// Adds every Altinn API client together with both adapters.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="maskinportenConfiguration">Configuration for the Altinn token exchange.</param>
    /// <param name="altinnConfiguration">
    /// Determines which Altinn instance to target. Required unless the host environment is Production or Staging.
    /// </param>
    /// <remarks>
    /// A convenience wrapper over <see cref="AddAltinn"/>. Prefer that when you only need some of
    /// the APIs, or need per-API scopes.
    /// </remarks>
    public static IServiceCollection AddAltinnAdapter(
        this IServiceCollection services,
        IWebHostEnvironment hostEnvironment,
        MaskinportenConfiguration maskinportenConfiguration,
        AltinnConfiguration? altinnConfiguration = null
    )
    {
        services
            .AddAltinn(hostEnvironment, maskinportenConfiguration, altinnConfiguration)
            .AddAllClients()
            .AddAltinnAdapter()
            .AddMeldingerAdapter();

        return services;
    }

    /// <summary>
    /// Adds every Altinn API client, without the adapters.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="maskinportenConfiguration">Configuration for the Altinn token exchange.</param>
    /// <param name="altinnConfiguration">
    /// Determines which Altinn instance to target. Required unless the host environment is Production or Staging.
    /// </param>
    /// <remarks>
    /// A convenience wrapper over <see cref="AddAltinn"/>. Prefer that when you only need some of
    /// the APIs, or need per-API scopes.
    /// </remarks>
    public static IServiceCollection AddAltinnApiClients(
        this IServiceCollection services,
        IWebHostEnvironment hostEnvironment,
        MaskinportenConfiguration maskinportenConfiguration,
        AltinnConfiguration? altinnConfiguration = null
    )
    {
        services
            .AddAltinn(hostEnvironment, maskinportenConfiguration, altinnConfiguration)
            .AddAllClients();

        return services;
    }

    /// <summary>
    /// Requires that a registered client ends up with a usable scope set, so a missing scope is
    /// caught at startup rather than as a rejected token at the first request.
    /// </summary>
    /// <remarks>
    /// Validation is deferred rather than checked here, because a client can be registered
    /// implicitly by an adapter and configured afterwards -- the scopes are not necessarily known
    /// at the point the client is added.
    /// </remarks>
    private static void RequireScopes(this AltinnBuilder builder, string clientName)
    {
        // A local Altinn instance issues test tokens directly, so Maskinporten scopes are moot.
        if (builder.Resolution.Target == AltinnEnvironment.Local)
        {
            return;
        }

        builder
            .Services.AddOptions<AltinnClientOptions>(clientName)
            .Validate(
                options => options.Scopes is { Length: > 0 },
                $"No Maskinporten scopes are configured for the '{clientName}' Altinn client. "
                    + $"Set {nameof(AltinnClientOptions.Scopes)} when adding it, or set "
                    + $"{nameof(MaskinportenConfiguration)}.{nameof(MaskinportenConfiguration.Scopes)} "
                    + "as a shared default for every client."
            )
            .ValidateOnStart();
    }

    private static IEnumerable<(string ClientName, Uri DefaultUrl)> DefaultUrls(
        ResolvedAltinnUrls urls
    )
    {
        yield return (AltinnClients.Storage, urls.StorageUrl);
        yield return (AltinnClients.Events, urls.EventsUrl);
        yield return (AltinnClients.Apps, urls.AppBaseUrl);
        yield return (AltinnClients.Correspondence, urls.CorrespondenceUrl);
        yield return (AltinnClients.Dialogporten, urls.DialogportenUrl);
    }

    /// <summary>
    /// The authentication and Maskinporten clients are what turn credentials into a token, so every
    /// other client depends on them regardless of which APIs were added.
    /// </summary>
    private static void AddAuthenticationInfrastructure(this AltinnBuilder builder)
    {
        if (!builder.TryRegister("Authentication"))
        {
            return;
        }

        var services = builder.Services;

        services
            .AddHttpClient(
                AltinnAuthenticationApiClientKey,
                client => client.BaseAddress = builder.Resolution.Urls.AuthenticationUrl
            )
            .AddStandardResilienceHandler();
        services
            .AddHttpClient(
                MaskinportenApiClientKey,
                client => client.BaseAddress = builder.Resolution.Urls.MaskinportenUrl
            )
            .AddStandardResilienceHandler();

        services.AddScoped<AuthenticationRequestAdapter>();
        services.AddScoped(serviceProvider =>
        {
            var adapter = serviceProvider.GetRequiredService<AuthenticationRequestAdapter>();
            adapter.BaseUrl = builder.Resolution.Urls.AuthenticationUrl.ToString();
            return new AuthenticationApiClient(adapter);
        });

        services.AddTransient<IAltinnAuthenticationClient, AltinnAuthenticationClient>();
        services.AddTransient<IMaskinportenClient, MaskinportenClient>();
    }

    /// <summary>
    /// Applies a client's configuration and validates any base URL override against the target
    /// environment, so a misconfiguration fails at startup rather than at the first request.
    /// </summary>
    private static AltinnBuilder Configure(
        this IAltinnBuilder builder,
        string clientName,
        Action<AltinnClientOptions>? configure
    )
    {
        var altinn = builder.AsAltinnBuilder();

        if (configure is null)
        {
            return altinn;
        }

        var probe = new AltinnClientOptions();
        configure(probe);

        if (probe.BaseUrl is { } overridden)
        {
            altinn.GuardBaseUrlOverride(clientName, overridden);
        }

        altinn.Services.Configure(clientName, configure);

        return altinn;
    }

    /// <summary>
    /// Applies the same rule to a per-client base URL as to <see cref="AltinnUrlOverrides"/>:
    /// forbidden in Production, warned about elsewhere.
    /// </summary>
    private static void GuardBaseUrlOverride(
        this AltinnBuilder builder,
        string clientName,
        Uri overridden
    )
    {
        var defaultUrl = DefaultUrls(builder.Resolution.Urls)
            .First(entry => entry.ClientName == clientName)
            .DefaultUrl;

        if (overridden == defaultUrl)
        {
            return;
        }

        if (builder.IsProductionHost)
        {
            throw new InvalidOperationException(
                "The host environment is Production, so overriding Altinn base URLs is not permitted. "
                    + $"Offending override(s): {clientName}.{nameof(AltinnClientOptions.BaseUrl)}. "
                    + "Remove the override, or run the application in a non-production environment."
            );
        }

        builder.Overrides.Add($"{clientName}.{nameof(AltinnClientOptions.BaseUrl)}");
        builder.EnsureOverrideWarningRegistered();
    }

    private static void EnsureOverrideWarningRegistered(this AltinnBuilder builder)
    {
        if (builder.TryRegister("OverrideWarning"))
        {
            builder.Services.AddHostedService<AltinnUrlOverrideWarningService>();
        }
    }

    /// <summary>
    /// Registers a Kiota request adapter together with the generated client built on top of it,
    /// pinning the adapter's base URL to the client's effective value.
    /// </summary>
    private static void AddGeneratedClient<TAdapter, TClient>(
        this AltinnBuilder builder,
        string clientName,
        Func<TAdapter, TClient> createClient
    )
        where TAdapter : class, IRequestAdapter
        where TClient : class
    {
        builder.Services.AddScoped<TAdapter>();
        builder.Services.AddScoped(serviceProvider =>
        {
            var adapter = serviceProvider.GetRequiredService<TAdapter>();
            adapter.BaseUrl = serviceProvider.EffectiveBaseUrl(clientName).ToString();
            return createClient(adapter);
        });
    }

    private static void AddAltinnHttpClient(
        this AltinnBuilder builder,
        string httpClientKey,
        string clientName
    )
    {
        builder
            .Services.AddHttpClient(
                httpClientKey,
                (serviceProvider, client) =>
                    client.BaseAddress = serviceProvider.EffectiveBaseUrl(clientName)
            )
            .AddStandardResilienceHandler();
    }

    private static Uri EffectiveBaseUrl(this IServiceProvider serviceProvider, string clientName)
    {
        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<AltinnClientOptions>>()
            .Get(clientName);

        return options.BaseUrl
            ?? throw new InvalidOperationException(
                $"No base URL resolved for the '{clientName}' Altinn client."
            );
    }

    private static AltinnBuilder AsAltinnBuilder(this IAltinnBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder as AltinnBuilder
            ?? throw new InvalidOperationException(
                $"The Altinn builder must be the one returned by {nameof(AddAltinn)}."
            );
    }
}
