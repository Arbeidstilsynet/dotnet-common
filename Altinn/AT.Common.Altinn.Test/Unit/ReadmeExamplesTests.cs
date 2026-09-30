using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

/// <summary>
/// Compiles and runs the registration snippets from the README, so the documentation cannot drift
/// away from the public API without a test failing.
/// </summary>
public class ReadmeExamplesTests
{
    private static readonly MaskinportenConfiguration Credentials = new()
    {
        Scopes = ["altinn:serviceowner/instances.read"],
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

    [Fact]
    public void DependencyInjectionSetup_Example()
    {
        var services = new ServiceCollection();

        services
            .AddAltinn(Environment(Environments.Staging), Credentials)
            .AddStorage(o => o.Scopes = ["altinn:serviceowner/instances.read"])
            .AddEvents(o => o.Scopes = ["altinn:events.subscribe"])
            .AddSubscriptionAdapter();

        var provider = services.BuildServiceProvider();

        provider.GetService<IAltinnSubscriptionAdapter>().ShouldNotBeNull();
        provider.GetService<IAltinnStorageClient>().ShouldNotBeNull();
        provider.GetService<IAltinnEventsClient>().ShouldNotBeNull();
    }

    [Fact]
    public void EveryAddMethodInTheTable_Exists()
    {
        var services = new ServiceCollection();

        services
            .AddAltinn(Environment(Environments.Staging), Credentials)
            .AddStorage()
            .AddEvents()
            .AddApps()
            .AddCorrespondence()
            .AddDialogporten()
            .AddSubscriptionAdapter()
            .AddStorageAdapter()
            .AddMeldingerAdapter()
            .AddAllClients();

        var provider = services.BuildServiceProvider();

        provider.GetService<IAltinnStorageClient>().ShouldNotBeNull();
        provider.GetService<IAltinnEventsClient>().ShouldNotBeNull();
        provider.GetService<IAltinnAppsClient>().ShouldNotBeNull();
        provider.GetService<IAltinnCorrespondenceClient>().ShouldNotBeNull();
        provider.GetService<IAltinnDialogportenClient>().ShouldNotBeNull();
        provider.GetService<IAltinnSubscriptionAdapter>().ShouldNotBeNull();
        provider.GetService<IAltinnStorageAdapter>().ShouldNotBeNull();
        provider.GetService<IAltinnMeldingerAdapter>().ShouldNotBeNull();
    }

    [Fact]
    public void ConfiguringAfterAnAdapter_Example()
    {
        var services = new ServiceCollection();

        services
            .AddAltinn(Environment(Environments.Staging), Credentials)
            .AddSubscriptionAdapter()
            .AddStorage(o => o.Scopes = ["altinn:serviceowner/instances.read"]);

        services.BuildServiceProvider().GetService<IAltinnSubscriptionAdapter>().ShouldNotBeNull();
    }

    [Fact]
    public void RegisterEverythingInOneCall_Example()
    {
        var services = new ServiceCollection();

        services.AddAltinnAdapter(Environment(Environments.Staging), Credentials);
        services.AddAltinnApiClients(Environment(Environments.Staging), Credentials);

        var provider = services.BuildServiceProvider();

        provider.GetService<IAltinnSubscriptionAdapter>().ShouldNotBeNull();
        provider.GetService<IAltinnStorageAdapter>().ShouldNotBeNull();
    }

    [Fact]
    public void SharedAndPerClientScopes_Example()
    {
        var services = new ServiceCollection();

        services
            .AddAltinn(
                Environment(Environments.Staging),
                new MaskinportenConfiguration
                {
                    Scopes = ["altinn:serviceowner/instances.read"],
                    PrivateKey = "some-private-key",
                    CertificateChain = "some-certificate-chain",
                    IntegrationId = "some-integration-id",
                }
            )
            .AddStorage()
            .AddEvents(o => o.Scopes = ["altinn:events.subscribe"]);

        services.BuildServiceProvider().GetService<IAltinnStorageClient>().ShouldNotBeNull();
    }

    [Fact]
    public void StatingTheTargetExplicitly_Example()
    {
        var services = new ServiceCollection();

        services.AddAltinn(
            Environment(Environments.Development),
            Credentials,
            new AltinnConfiguration { Environment = AltinnEnvironment.Tt02 }
        );

        services.BuildServiceProvider().ShouldNotBeNull();
    }

    [Fact]
    public void PlatformUrlOverride_Example()
    {
        var services = new ServiceCollection();

        services.AddAltinn(
            Environment(Environments.Development),
            Credentials,
            new AltinnConfiguration
            {
                Environment = AltinnEnvironment.Tt02,
                Overrides = new AltinnUrlOverrides
                {
                    PlatformUrl = new Uri("http://localhost:1234/"),
                },
            }
        );

        services.BuildServiceProvider().ShouldNotBeNull();
    }

    [Fact]
    public void PerClientBaseUrlOverride_Example()
    {
        var services = new ServiceCollection();

        services
            .AddAltinn(Environment(Environments.Staging), Credentials)
            .AddStorage(o => o.BaseUrl = new Uri("http://localhost:1234/storage/api/v1"));

        services.BuildServiceProvider().GetService<IAltinnStorageClient>().ShouldNotBeNull();
    }

    [Fact]
    public void PublicModels_AreOrdinarySystemTextJsonRecords()
    {
        // The README promises these are safe to return from a consumer's own API.
        var instance = new AltinnInstance
        {
            Id = "1/abc",
            AppId = "dat/app",
            Data = [new DataElement { Id = "d1", DataType = "model" }],
        };

        var json = System.Text.Json.JsonSerializer.Serialize(
            instance,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)
        );

        json.ShouldContain("\"id\":\"1/abc\"");
        json.ShouldNotContain("additionalData");
    }

    [Fact]
    public void ProblemDetailsErrorHandling_Example()
    {
        // Mirrors the README's catch block, so the documented shape keeps compiling.
        var logged = new List<string?>();

        try
        {
            throw new Microsoft.Kiota.Abstractions.ApiException("boom");
        }
        catch (Microsoft.Kiota.Abstractions.ApiException e)
        {
            var problem = e.GetAltinnProblemDetails();

            logged.Add(problem?.Status?.ToString());
            logged.Add(problem?.Title);
            logged.Add(problem?.Detail);
            logged.Add(problem?.TraceId);
        }

        logged.ShouldAllBe(entry => entry == null);
    }
}
