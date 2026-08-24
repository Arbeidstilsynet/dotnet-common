using Arbeidstilsynet.Common.Altinn.Apps;
using Arbeidstilsynet.Common.Altinn.Authentication;
using Arbeidstilsynet.Common.Altinn.Correspondence;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Dialogporten;
using Arbeidstilsynet.Common.Altinn.Events;
using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Implementation.Authentication;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Arbeidstilsynet.Common.Altinn.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

/// <summary>
/// Asserts that every generated client is given the base URL resolved for the target environment.
/// </summary>
/// <remarks>
/// The generated clients fall back to the server declared in their specification when no base URL
/// is set, and every specification declares TT02. A wiring regression would therefore not fail
/// loudly -- it would quietly point a production application at the test environment.
/// </remarks>
public class DependencyInjectionBaseUrlTests
{
    private readonly MaskinportenConfiguration _maskinportenConfiguration = new()
    {
        Scopes = ["scope1"],
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

    private IServiceProvider BuildProvider(
        string environmentName,
        AltinnConfiguration? configuration = null
    )
    {
        var services = new ServiceCollection();
        services.AddAltinnAdapter(
            Environment(environmentName),
            _maskinportenConfiguration,
            configuration
        );
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Resolves the generated client first so that registration assigns the base URL, then reads it
    /// back from the request adapter the client was built with.
    /// </summary>
    private static string BaseUrlOf<TClient, TAdapter>(IServiceProvider serviceProvider)
        where TClient : notnull
        where TAdapter : notnull, IRequestAdapter
    {
        _ = serviceProvider.GetRequiredService<TClient>();

        return serviceProvider.GetRequiredService<TAdapter>().BaseUrl!;
    }

    [Fact]
    public void Production_PinsEveryClientToProduction()
    {
        using var scope = BuildProvider(Environments.Production).CreateScope();
        var services = scope.ServiceProvider;

        BaseUrlOf<StorageApiClient, StorageRequestAdapter>(services)
            .ShouldBe("https://platform.altinn.no/storage/api/v1");
        BaseUrlOf<EventsApiClient, EventsRequestAdapter>(services)
            .ShouldBe("https://platform.altinn.no/events/api/v1");
        BaseUrlOf<AuthenticationApiClient, AuthenticationRequestAdapter>(services)
            .ShouldBe("https://platform.altinn.no/authentication/api/v1");
        BaseUrlOf<CorrespondenceApiClient, CorrespondenceRequestAdapter>(services)
            .ShouldBe("https://platform.altinn.no");
        BaseUrlOf<DialogportenApiClient, DialogportenRequestAdapter>(services)
            .ShouldBe("https://platform.altinn.no/dialogporten");
        BaseUrlOf<AppsApiClient, AppsRequestAdapter>(services)
            .ShouldBe("https://dat.apps.altinn.no");
    }

    [Fact]
    public void Staging_PinsEveryClientToTt02()
    {
        using var scope = BuildProvider(Environments.Staging).CreateScope();
        var services = scope.ServiceProvider;

        BaseUrlOf<StorageApiClient, StorageRequestAdapter>(services)
            .ShouldBe("https://platform.tt02.altinn.no/storage/api/v1");
        BaseUrlOf<CorrespondenceApiClient, CorrespondenceRequestAdapter>(services)
            .ShouldBe("https://platform.tt02.altinn.no");
        BaseUrlOf<AppsApiClient, AppsRequestAdapter>(services)
            .ShouldBe("https://dat.apps.tt02.altinn.no");
    }

    [Fact]
    public void TargetingLocal_PinsEveryClientToTheLocalInstance()
    {
        using var scope = BuildProvider(
                Environments.Development,
                new AltinnConfiguration { Environment = AltinnEnvironment.Local }
            )
            .CreateScope();
        var services = scope.ServiceProvider;

        BaseUrlOf<StorageApiClient, StorageRequestAdapter>(services)
            .ShouldBe("http://local.altinn.cloud:8000/storage/api/v1");
        BaseUrlOf<AppsApiClient, AppsRequestAdapter>(services)
            .ShouldBe("http://local.altinn.cloud:8000");
    }

    [Fact]
    public void OverriddenPlatformUrl_IsAppliedToEveryPlatformClient()
    {
        using var scope = BuildProvider(
                Environments.Development,
                new AltinnConfiguration
                {
                    Environment = AltinnEnvironment.Tt02,
                    Overrides = new AltinnUrlOverrides
                    {
                        PlatformUrl = new Uri("http://localhost:1234/"),
                    },
                }
            )
            .CreateScope();
        var services = scope.ServiceProvider;

        BaseUrlOf<StorageApiClient, StorageRequestAdapter>(services)
            .ShouldBe("http://localhost:1234/storage/api/v1");
        BaseUrlOf<DialogportenApiClient, DialogportenRequestAdapter>(services)
            .ShouldBe("http://localhost:1234/dialogporten");
    }

    [Fact]
    public void NoClientFallsBackToTheSpecificationsDeclaredServer()
    {
        using var scope = BuildProvider(Environments.Production).CreateScope();
        var services = scope.ServiceProvider;

        string[] baseUrls =
        [
            BaseUrlOf<StorageApiClient, StorageRequestAdapter>(services),
            BaseUrlOf<EventsApiClient, EventsRequestAdapter>(services),
            BaseUrlOf<AuthenticationApiClient, AuthenticationRequestAdapter>(services),
            BaseUrlOf<CorrespondenceApiClient, CorrespondenceRequestAdapter>(services),
            BaseUrlOf<DialogportenApiClient, DialogportenRequestAdapter>(services),
            BaseUrlOf<AppsApiClient, AppsRequestAdapter>(services),
        ];

        baseUrls.ShouldAllBe(baseUrl => !baseUrl.Contains("tt02"));
    }

    [Fact]
    public async Task AuthenticationProvider_AttachesTheAltinnTokenAsBearer()
    {
        var tokenProvider = Substitute.For<IAltinnTokenProvider>();
        tokenProvider.GetToken(Arg.Any<CancellationToken>()).Returns("altinn-token");

        var request = new RequestInformation
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://platform.altinn.no/storage/api/v1/instances"),
        };

        await new AltinnAuthenticationProvider(tokenProvider).AuthenticateRequestAsync(request);

        request.Headers["Authorization"].ShouldContain("Bearer altinn-token");
    }

    [Fact]
    public async Task AuthenticationClient_SendsTheExternalTokenRatherThanAnAltinnToken()
    {
        // The authentication API is what issues Altinn tokens, so its adapter authenticates
        // anonymously and the Maskinporten token is supplied per request. If it ever resolved the
        // Altinn authentication provider instead, obtaining a token would recurse.
        var requestAdapter = Substitute.For<IRequestAdapter>();
        requestAdapter.BaseUrl = "https://platform.altinn.no/authentication/api/v1";
        requestAdapter
            .SendPrimitiveAsync<string>(
                Arg.Any<RequestInformation>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns("altinn-token");

        var sut = new AltinnAuthenticationClient(new AuthenticationApiClient(requestAdapter));

        var result = await sut.ExchangeToken("maskinporten-token");

        result.ShouldBe("altinn-token");

        var request = (RequestInformation)
            requestAdapter
                .ReceivedCalls()
                .Last(call => call.GetArguments().FirstOrDefault() is RequestInformation)
                .GetArguments()[0]!;

        request.Headers["Authorization"].ShouldContain("Bearer maskinporten-token");
        request
            .URI.ToString()
            .ShouldBe("https://platform.altinn.no/authentication/api/v1/exchange/maskinporten");
    }
}
