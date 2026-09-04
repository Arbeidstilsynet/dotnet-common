using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation.Configuration;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class DependencyInjectionExtensionTests
{
    private readonly IWebHostEnvironment _production = CreateEnvironment(Environments.Production);
    private readonly IWebHostEnvironment _staging = CreateEnvironment(Environments.Staging);
    private readonly IWebHostEnvironment _development = CreateEnvironment(Environments.Development);
    private readonly IWebHostEnvironment _customEnvironment = CreateEnvironment("QA");

    private readonly MaskinportenConfiguration _maskinportenConfiguration = new()
    {
        Scopes = ["scope1", "scope2"],
        PrivateKey = "some-private-key",
        CertificateChain = "some-certificate-chain",
        IntegrationId = "some-integration-id",
    };

    private static IWebHostEnvironment CreateEnvironment(string environmentName)
    {
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(environmentName);
        return environment;
    }

    private ResolvedAltinnUrls Resolve(
        IWebHostEnvironment hostEnvironment,
        AltinnConfiguration? configuration = null
    )
    {
        var services = new ServiceCollection();
        services.AddAltinnAdapter(hostEnvironment, _maskinportenConfiguration, configuration);

        return services.BuildServiceProvider().GetRequiredService<ResolvedAltinnUrls>();
    }

    [Fact]
    public void Production_ResolvesProductionUrls()
    {
        var urls = Resolve(_production);

        urls.AuthenticationUrl.ShouldBe(
            new Uri("https://platform.altinn.no/authentication/api/v1")
        );
        urls.StorageUrl.ShouldBe(new Uri("https://platform.altinn.no/storage/api/v1"));
        urls.EventsUrl.ShouldBe(new Uri("https://platform.altinn.no/events/api/v1"));
        urls.CorrespondenceUrl.ShouldBe(new Uri("https://platform.altinn.no"));
        urls.DialogportenUrl.ShouldBe(new Uri("https://platform.altinn.no/dialogporten"));
        urls.AppBaseUrl.ShouldBe(new Uri("https://dat.apps.altinn.no/"));
        urls.MaskinportenUrl.ShouldBe(new Uri("https://maskinporten.no/"));
    }

    [Fact]
    public void Staging_DefaultsToTt02Urls()
    {
        var urls = Resolve(_staging);

        urls.StorageUrl.ShouldBe(new Uri("https://platform.tt02.altinn.no/storage/api/v1"));
        urls.CorrespondenceUrl.ShouldBe(new Uri("https://platform.tt02.altinn.no"));
        urls.DialogportenUrl.ShouldBe(new Uri("https://platform.tt02.altinn.no/dialogporten"));
        urls.AppBaseUrl.ShouldBe(new Uri("https://dat.apps.tt02.altinn.no/"));
        urls.MaskinportenUrl.ShouldBe(new Uri("https://test.maskinporten.no/"));
    }

    [Fact]
    public void Development_TargetingTt02_ResolvesTt02Urls()
    {
        var urls = Resolve(
            _development,
            new AltinnConfiguration { Environment = AltinnEnvironment.Tt02 }
        );

        urls.StorageUrl.ShouldBe(new Uri("https://platform.tt02.altinn.no/storage/api/v1"));
        urls.AppBaseUrl.ShouldBe(new Uri("https://dat.apps.tt02.altinn.no/"));
    }

    [Fact]
    public void Development_TargetingLocal_ResolvesLocalUrls()
    {
        var urls = Resolve(
            _development,
            new AltinnConfiguration { Environment = AltinnEnvironment.Local }
        );

        urls.AuthenticationUrl.ShouldBe(
            new Uri("http://local.altinn.cloud:8000/authentication/api/v1")
        );
        urls.StorageUrl.ShouldBe(new Uri("http://local.altinn.cloud:8000/storage/api/v1"));
        urls.AppBaseUrl.ShouldBe(new Uri("http://local.altinn.cloud:8000/"));
    }

    [Fact]
    public void Development_WithoutEnvironment_Throws()
    {
        var act = () => Resolve(_development);

        var exception = act.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("No Altinn environment configured");
    }

    [Fact]
    public void CustomEnvironment_WithoutEnvironment_Throws()
    {
        var act = () => Resolve(_customEnvironment);

        var exception = act.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("QA");
    }

    [Fact]
    public void CustomEnvironment_TargetingTt02_ResolvesTt02Urls()
    {
        var urls = Resolve(
            _customEnvironment,
            new AltinnConfiguration { Environment = AltinnEnvironment.Tt02 }
        );

        urls.StorageUrl.ShouldBe(new Uri("https://platform.tt02.altinn.no/storage/api/v1"));
    }

    [Fact]
    public void Production_WithOverride_Throws()
    {
        var act = () =>
            Resolve(
                _production,
                new AltinnConfiguration
                {
                    Overrides = new AltinnUrlOverrides
                    {
                        PlatformUrl = new Uri("http://localhost:1234/"),
                    },
                }
            );

        var exception = act.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("not permitted");
    }

    [Fact]
    public void Production_TargetingTt02_Throws()
    {
        var act = () =>
            Resolve(_production, new AltinnConfiguration { Environment = AltinnEnvironment.Tt02 });

        var exception = act.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("must not target a test instance");
    }

    [Fact]
    public void Production_WithRedundantOverride_IsAllowed()
    {
        // An override that matches the resolved default is not treated as an override.
        var urls = Resolve(
            _production,
            new AltinnConfiguration
            {
                Overrides = new AltinnUrlOverrides
                {
                    PlatformUrl = new Uri("https://platform.altinn.no/"),
                },
            }
        );

        urls.StorageUrl.ShouldBe(new Uri("https://platform.altinn.no/storage/api/v1"));
    }

    [Fact]
    public void PlatformUrlOverride_DerivesAllPlatformApis()
    {
        var urls = Resolve(
            _development,
            new AltinnConfiguration
            {
                Environment = AltinnEnvironment.Tt02,
                Overrides = new AltinnUrlOverrides
                {
                    PlatformUrl = new Uri("http://localhost:1234/"),
                },
            }
        );

        urls.AuthenticationUrl.ShouldBe(new Uri("http://localhost:1234/authentication/api/v1"));
        urls.StorageUrl.ShouldBe(new Uri("http://localhost:1234/storage/api/v1"));
        urls.EventsUrl.ShouldBe(new Uri("http://localhost:1234/events/api/v1"));
        urls.CorrespondenceUrl.ShouldBe(new Uri("http://localhost:1234"));
        urls.DialogportenUrl.ShouldBe(new Uri("http://localhost:1234/dialogporten"));

        // Not derived from the platform URL, so unaffected.
        urls.AppBaseUrl.ShouldBe(new Uri("https://dat.apps.tt02.altinn.no/"));
    }

    [Fact]
    public void PerApiOverride_TakesPrecedenceOverPlatformUrl()
    {
        var urls = Resolve(
            _development,
            new AltinnConfiguration
            {
                Environment = AltinnEnvironment.Tt02,
                Overrides = new AltinnUrlOverrides
                {
                    PlatformUrl = new Uri("http://localhost:1234/"),
                    StorageUrl = new Uri("http://localhost:5678/storage"),
                },
            }
        );

        urls.StorageUrl.ShouldBe(new Uri("http://localhost:5678/storage"));
        urls.EventsUrl.ShouldBe(new Uri("http://localhost:1234/events/api/v1"));
    }

    [Fact]
    public void OrgId_IsReflectedInAppBaseUrl()
    {
        var urls = Resolve(_production, new AltinnConfiguration { OrgId = "my-org" });

        urls.AppBaseUrl.ShouldBe(new Uri("https://my-org.apps.altinn.no/"));
    }

    [Fact]
    public void TargetingLocal_RegistersLocalTokenProvider()
    {
        var services = new ServiceCollection();
        services.AddAltinnAdapter(
            _development,
            _maskinportenConfiguration,
            new AltinnConfiguration { Environment = AltinnEnvironment.Local }
        );

        var tokenProvider = services
            .BuildServiceProvider()
            .GetRequiredService<IAltinnTokenProvider>();

        tokenProvider.GetType().Name.ShouldBe("LocalAltinnTokenProvider");
    }

    [Fact]
    public void TargetingTt02_RegistersMaskinportenTokenProvider()
    {
        var services = new ServiceCollection();
        services.AddAltinnAdapter(
            _development,
            _maskinportenConfiguration,
            new AltinnConfiguration { Environment = AltinnEnvironment.Tt02 }
        );

        var tokenProvider = services
            .BuildServiceProvider()
            .GetRequiredService<IAltinnTokenProvider>();

        tokenProvider.GetType().Name.ShouldBe("AltinnTokenProvider");
    }

    [Fact]
    public void Staging_WithOverride_RegistersWarningService()
    {
        var services = new ServiceCollection();
        services.AddAltinnAdapter(
            _staging,
            _maskinportenConfiguration,
            new AltinnConfiguration
            {
                Overrides = new AltinnUrlOverrides
                {
                    PlatformUrl = new Uri("http://localhost:1234/"),
                },
            }
        );

        var hostedServices = services.BuildServiceProvider().GetServices<IHostedService>();

        hostedServices.ShouldContain(service =>
            service.GetType().Name == "AltinnUrlOverrideWarningService"
        );
    }

    [Fact]
    public void Staging_WithoutOverride_DoesNotRegisterWarningService()
    {
        var services = new ServiceCollection();
        services.AddAltinnAdapter(_staging, _maskinportenConfiguration);

        var hostedServices = services.BuildServiceProvider().GetServices<IHostedService>();

        hostedServices.ShouldBeEmpty();
    }
}
