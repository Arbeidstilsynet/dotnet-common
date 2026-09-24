using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Arbeidstilsynet.Common.Altinn.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

/// <summary>
/// Covers the builder surface: which clients get registered, and how per-client configuration is
/// resolved.
/// </summary>
public class AltinnBuilderTests
{
    private static readonly MaskinportenConfiguration Credentials = new()
    {
        Scopes = ["shared:scope"],
        PrivateKey = "some-private-key",
        CertificateChain = "some-certificate-chain",
        IntegrationId = "some-integration-id",
    };

    private static IWebHostEnvironment Environment(string environmentName)
    {
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(environmentName);
        return environment;
    }

    private static IAltinnBuilder Builder(
        ServiceCollection services,
        string? environmentName = null
    ) => services.AddAltinn(Environment(environmentName ?? Environments.Staging), Credentials);

    private static AltinnClientOptions OptionsFor(IServiceProvider services, string clientName) =>
        services.GetRequiredService<IOptionsMonitor<AltinnClientOptions>>().Get(clientName);

    [Fact]
    public void AddingOneClient_DoesNotRegisterTheOthers()
    {
        var services = new ServiceCollection();
        Builder(services).AddStorage();

        var provider = services.BuildServiceProvider();

        provider.GetService<IAltinnStorageClient>().ShouldNotBeNull();
        provider.GetService<IAltinnEventsClient>().ShouldBeNull();
        provider.GetService<IAltinnCorrespondenceClient>().ShouldBeNull();
        provider.GetService<IAltinnDialogportenClient>().ShouldBeNull();
        provider.GetService<IAltinnAppsClient>().ShouldBeNull();
    }

    [Fact]
    public void AddAltinn_AloneRegistersNoApiClients()
    {
        var services = new ServiceCollection();
        Builder(services);

        var provider = services.BuildServiceProvider();

        provider.GetService<IAltinnStorageClient>().ShouldBeNull();
        provider.GetService<IAltinnSubscriptionAdapter>().ShouldBeNull();

        // The token pipeline is shared plumbing, so it is always available.
        provider.GetService<IAltinnTokenProvider>().ShouldNotBeNull();
        provider.GetService<IMaskinportenClient>().ShouldNotBeNull();
    }

    [Fact]
    public void AddSubscriptionAdapter_PullsInTheClientsItDependsOn()
    {
        var services = new ServiceCollection();
        Builder(services).AddSubscriptionAdapter();

        var provider = services.BuildServiceProvider();

        provider.GetService<IAltinnSubscriptionAdapter>().ShouldNotBeNull();
        provider.GetService<IAltinnStorageClient>().ShouldNotBeNull();
        provider.GetService<IAltinnEventsClient>().ShouldNotBeNull();

        // ...but nothing beyond them.
        provider.GetService<IAltinnCorrespondenceClient>().ShouldBeNull();
    }

    [Fact]
    public void AddMeldingerAdapter_PullsInCorrespondence()
    {
        var services = new ServiceCollection();
        Builder(services).AddMeldingerAdapter();

        var provider = services.BuildServiceProvider();

        provider.GetService<IAltinnMeldingerAdapter>().ShouldNotBeNull();
        provider.GetService<IAltinnCorrespondenceClient>().ShouldNotBeNull();
        provider.GetService<IAltinnStorageClient>().ShouldBeNull();
    }

    [Fact]
    public void AddStorageAdapter_PullsInOnlyStorage()
    {
        var services = new ServiceCollection();
        Builder(services).AddStorageAdapter();

        var provider = services.BuildServiceProvider();

        provider.GetService<IAltinnStorageAdapter>().ShouldNotBeNull();
        provider.GetService<IAltinnStorageClient>().ShouldNotBeNull();
        provider.GetService<IAltinnEventsClient>().ShouldBeNull();
        provider.GetService<IAltinnCorrespondenceClient>().ShouldBeNull();
    }

    [Fact]
    public void AddingStorageAdapterAndStorageClient_RegistersEachOnce()
    {
        var services = new ServiceCollection();
        Builder(services).AddStorageAdapter().AddStorage().AddStorageAdapter();

        var provider = services.BuildServiceProvider();

        provider.GetServices<IAltinnStorageAdapter>().Count().ShouldBe(1);
        provider.GetServices<IAltinnStorageClient>().Count().ShouldBe(1);
    }

    [Fact]
    public void ClientWithoutScopes_FallsBackToTheSharedScopes()
    {
        var services = new ServiceCollection();
        Builder(services).AddStorage();

        var provider = services.BuildServiceProvider();

        OptionsFor(provider, AltinnClients.Storage).Scopes.ShouldBe(["shared:scope"]);
    }

    [Fact]
    public void ClientScopes_AreIndependentOfEachOther()
    {
        var services = new ServiceCollection();
        Builder(services)
            .AddStorage(o => o.Scopes = ["altinn:serviceowner/instances.read"])
            .AddEvents(o => o.Scopes = ["altinn:events.subscribe"])
            .AddCorrespondence();

        var provider = services.BuildServiceProvider();

        OptionsFor(provider, AltinnClients.Storage)
            .Scopes.ShouldBe(["altinn:serviceowner/instances.read"]);
        OptionsFor(provider, AltinnClients.Events).Scopes.ShouldBe(["altinn:events.subscribe"]);
        OptionsFor(provider, AltinnClients.Correspondence).Scopes.ShouldBe(["shared:scope"]);
    }

    [Fact]
    public void ConfiguringAClientAfterAnAdapterAddedIt_StillApplies()
    {
        // AddSubscriptionAdapter registers storage implicitly. Configuring it afterwards must not be
        // silently ignored, or the scopes an application asked for would be quietly dropped.
        var services = new ServiceCollection();
        Builder(services)
            .AddSubscriptionAdapter()
            .AddStorage(o => o.Scopes = ["altinn:serviceowner/instances.read"]);

        var provider = services.BuildServiceProvider();

        OptionsFor(provider, AltinnClients.Storage)
            .Scopes.ShouldBe(["altinn:serviceowner/instances.read"]);
    }

    [Fact]
    public void AddingTheSameClientTwice_RegistersItOnce()
    {
        var services = new ServiceCollection();
        Builder(services).AddStorage().AddStorage().AddSubscriptionAdapter();

        var provider = services.BuildServiceProvider();

        provider.GetServices<IAltinnStorageClient>().Count().ShouldBe(1);
    }

    [Fact]
    public void PerClientBaseUrl_OverridesTheResolvedUrl()
    {
        var services = new ServiceCollection();
        Builder(services)
            .AddStorage(o => o.BaseUrl = new Uri("http://localhost:9999/storage/api/v1"))
            .AddEvents();

        var provider = services.BuildServiceProvider();

        OptionsFor(provider, AltinnClients.Storage)
            .BaseUrl.ShouldBe(new Uri("http://localhost:9999/storage/api/v1"));

        // Only the client that was overridden moves.
        OptionsFor(provider, AltinnClients.Events)
            .BaseUrl.ShouldBe(new Uri("https://platform.tt02.altinn.no/events/api/v1"));
    }

    [Fact]
    public void PerClientBaseUrl_ReachesTheGeneratedClient()
    {
        var services = new ServiceCollection();
        Builder(services)
            .AddStorage(o => o.BaseUrl = new Uri("http://localhost:9999/storage/api/v1"));

        using var scope = services.BuildServiceProvider().CreateScope();

        _ = scope.ServiceProvider.GetRequiredService<StorageApiClient>();

        scope
            .ServiceProvider.GetRequiredService<StorageRequestAdapter>()
            .BaseUrl.ShouldBe("http://localhost:9999/storage/api/v1");
    }

    [Fact]
    public void PerClientBaseUrl_InProduction_Throws()
    {
        var services = new ServiceCollection();

        // A per-client override must be subject to the same rule as AltinnUrlOverrides, otherwise
        // it would be a way to silently point production at a mock server.
        var act = () =>
            Builder(services, Environments.Production)
                .AddStorage(o => o.BaseUrl = new Uri("http://localhost:9999/"));

        var exception = act.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("Production");
        exception.Message.ShouldContain(AltinnClients.Storage);
    }

    [Fact]
    public void PerClientBaseUrl_MatchingTheResolvedDefault_IsAllowedInProduction()
    {
        var services = new ServiceCollection();

        var act = () =>
            Builder(services, Environments.Production)
                .AddStorage(o => o.BaseUrl = new Uri("https://platform.altinn.no/storage/api/v1"));

        act.ShouldNotThrow();
    }

    [Fact]
    public void PerClientBaseUrl_OutsideProduction_RegistersTheWarningService()
    {
        var services = new ServiceCollection();
        Builder(services).AddStorage(o => o.BaseUrl = new Uri("http://localhost:9999/"));

        services
            .Any(descriptor =>
                descriptor.ImplementationType?.Name == "AltinnUrlOverrideWarningService"
            )
            .ShouldBeTrue();
    }

    [Fact]
    public void NoOverrides_DoesNotRegisterTheWarningService()
    {
        var services = new ServiceCollection();
        Builder(services).AddStorage().AddEvents();

        services
            .Any(descriptor =>
                descriptor.ImplementationType?.Name == "AltinnUrlOverrideWarningService"
            )
            .ShouldBeFalse();
    }

    [Fact]
    public async Task EachClient_PresentsATokenForItsOwnScopes()
    {
        // The end-to-end check: configured scopes have to survive all the way into the token the
        // request adapter actually attaches, not merely into the options.
        var tokenProvider = Substitute.For<IAltinnTokenProvider>();
        tokenProvider
            .GetToken(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns("a-token");

        var services = new ServiceCollection();
        services.AddSingleton(tokenProvider);

        Builder(services)
            .AddStorage(o => o.Scopes = ["altinn:serviceowner/instances.read"])
            .AddEvents(o => o.Scopes = ["altinn:events.subscribe"]);

        services
            .AddHttpClient(DependencyInjectionExtensions.AltinnStorageApiClientKey)
            .ConfigurePrimaryHttpMessageHandler(() => new NoContentHandler());
        services
            .AddHttpClient(DependencyInjectionExtensions.AltinnEventsApiClientKey)
            .ConfigurePrimaryHttpMessageHandler(() => new NoContentHandler());

        using var scope = services.BuildServiceProvider().CreateScope();

        await scope
            .ServiceProvider.GetRequiredService<IAltinnStorageClient>()
            .GetInstances(new Model.Api.Request.InstanceQueryParameters { AppId = "dat/app" });

        await tokenProvider
            .Received(1)
            .GetToken(
                Arg.Is<IReadOnlyList<string>>(scopes =>
                    scopes!.Count == 1 && scopes[0] == "altinn:serviceowner/instances.read"
                ),
                Arg.Any<CancellationToken>()
            );

        await tokenProvider
            .DidNotReceive()
            .GetToken(
                Arg.Is<IReadOnlyList<string>>(scopes =>
                    scopes!.Contains("altinn:events.subscribe")
                ),
                Arg.Any<CancellationToken>()
            );
    }

    private sealed class NoContentHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{}",
                        System.Text.Encoding.UTF8,
                        "application/json"
                    ),
                }
            );
    }

    #region Scope validation

    private static readonly MaskinportenConfiguration CredentialsWithoutScopes = new()
    {
        PrivateKey = "some-private-key",
        CertificateChain = "some-certificate-chain",
        IntegrationId = "some-integration-id",
    };

    private static IServiceProvider ProviderWithoutFallbackScopes(
        Action<IAltinnBuilder> configure,
        string environmentName = "Staging",
        AltinnConfiguration? altinnConfiguration = null
    )
    {
        var services = new ServiceCollection();
        var builder = services.AddAltinn(
            Environment(environmentName),
            CredentialsWithoutScopes,
            altinnConfiguration
        );
        configure(builder);
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Runs startup validation and returns the failure message(s). A single failing client throws
    /// <see cref="OptionsValidationException"/>; several are aggregated.
    /// </summary>
    private static string StartupValidationFailure(IServiceProvider provider)
    {
        var act = () => provider.GetRequiredService<IStartupValidator>().Validate();

        var exception = act.ShouldThrow<Exception>();

        return exception is AggregateException aggregate
            ? string.Join(" | ", aggregate.InnerExceptions.Select(inner => inner.Message))
            : exception.Message;
    }

    [Fact]
    public void ClientWithNeitherOwnScopesNorFallback_FailsAtStartup()
    {
        var provider = ProviderWithoutFallbackScopes(altinn => altinn.AddStorage());

        var message = StartupValidationFailure(provider);

        message.ShouldContain(AltinnClients.Storage);
        message.ShouldContain(nameof(MaskinportenConfiguration.Scopes));
    }

    [Fact]
    public void ClientWithNeitherOwnScopesNorFallback_AlsoFailsWhenResolvedLazily()
    {
        // ValidateOnStart only fires if the host actually starts, so the same failure must surface
        // when the options are read -- otherwise a non-hosted consumer would get a rejected token
        // at the first request instead.
        var provider = ProviderWithoutFallbackScopes(altinn => altinn.AddStorage());

        var act = () => OptionsFor(provider, AltinnClients.Storage);

        act.ShouldThrow<OptionsValidationException>();
    }

    [Fact]
    public void ClientWithItsOwnScopes_NeedsNoFallback()
    {
        var provider = ProviderWithoutFallbackScopes(altinn =>
            altinn.AddStorage(o => o.Scopes = ["altinn:serviceowner/instances.read"])
        );

        Should.NotThrow(() => provider.GetRequiredService<IStartupValidator>().Validate());
        OptionsFor(provider, AltinnClients.Storage)
            .Scopes.ShouldBe(["altinn:serviceowner/instances.read"]);
    }

    [Fact]
    public void UnregisteredClients_AreNotValidated()
    {
        // Only the APIs an application actually added should be able to fail its startup.
        var provider = ProviderWithoutFallbackScopes(altinn =>
            altinn.AddStorage(o => o.Scopes = ["altinn:serviceowner/instances.read"])
        );

        Should.NotThrow(() => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    [Fact]
    public void ClientRegisteredByAnAdapter_IsStillValidated()
    {
        // The adapter adds storage and events implicitly, so both must be reported.
        var provider = ProviderWithoutFallbackScopes(altinn => altinn.AddSubscriptionAdapter());

        var message = StartupValidationFailure(provider);

        message.ShouldContain(AltinnClients.Storage);
        message.ShouldContain(AltinnClients.Events);
    }

    [Fact]
    public void ScopesConfiguredAfterAnAdapterAddedTheClient_SatisfyValidation()
    {
        var provider = ProviderWithoutFallbackScopes(altinn =>
            altinn
                .AddSubscriptionAdapter()
                .AddStorage(o => o.Scopes = ["altinn:serviceowner/instances.read"])
                .AddEvents(o => o.Scopes = ["altinn:events.subscribe"])
        );

        Should.NotThrow(() => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    [Fact]
    public void EmptyScopes_FallBackToTheSharedDefault()
    {
        var services = new ServiceCollection();
        Builder(services).AddStorage(o => o.Scopes = []);

        var provider = services.BuildServiceProvider();

        // An empty scope set is as unusable as an absent one, so it must not defeat the fallback.
        OptionsFor(provider, AltinnClients.Storage).Scopes.ShouldBe(["shared:scope"]);
    }

    [Fact]
    public void TargetingLocal_DoesNotRequireScopes()
    {
        // A local Altinn instance issues test tokens directly, so Maskinporten never sees a scope.
        var provider = ProviderWithoutFallbackScopes(
            altinn => altinn.AddStorage().AddEvents(),
            environmentName: "Development",
            altinnConfiguration: new AltinnConfiguration { Environment = AltinnEnvironment.Local }
        );

        Should.NotThrow(() => provider.GetRequiredService<IStartupValidator>().Validate());
        Should.NotThrow(() => OptionsFor(provider, AltinnClients.Storage));
    }

    #endregion
}
