using Altinn.App.Core.Helpers;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Model.Api;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.TestExtensions.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Scriban.Parsing;
using WireMock.Admin.Mappings;
using WireMock.Matchers;
using WireMock.Net.OpenApiParser.Settings;
using WireMock.Net.OpenApiParser.Types;
using WireMock.Server;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Altinn.Test.Setup;

public class AltinnApiTestFixture : TestBedFixture
{
    private readonly WireMockServer _server;

    private readonly IAltinnTokenProvider _tokenProvider = Substitute.For<IAltinnTokenProvider>();

    private readonly IWebHostEnvironment _webHostEnvironment =
        Substitute.For<IWebHostEnvironment>();

    internal const string SampleJwtToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30";

    internal static MaskinportenTokenResponse SampleMaskinportenTokenResponse = new()
    {
        AccessToken = "IxC0B76vlWl3fiQhAwZUmD0hr_PPwC9hSIXRdoUslPU=",
        TokenType = "Bearer",
        ExpiresIn = 120,
        Scope = "test:read",
    };

    public AltinnApiTestFixture()
    {
        _server = WireMockServer.Start();
        _server.AddMappings(
            "Unit/TestData/openapi/altinn-platform-events-v1.json",
            settings: new WireMockOpenApiParserSettings { DynamicExamples = false }
        );
        _server
            .WhenRequest(r =>
                r.WithPath("/authentication/api/v1/exchange/maskinporten")
                    .WithHeader("Authorization", new WildcardMatcher("Bearer *"))
                    .UsingGet()
            )
            .ThenRespondWith(r => r.WithStatusCode(200).WithBody(SampleJwtToken));

        _server.AddMappings(
            "Unit/TestData/openapi/altinn-platform-storage-v1.json",
            settings: new WireMockOpenApiParserSettings
            {
                ExampleValues = new DynamicDataGeneration(),
                PathPatternToUse = ExampleValueType.Value,
            }
        );
        _server
            .WhenRequest(r =>
                r.WithPath("/token")
                    .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                    .WithBody(
                        new FormUrlEncodedMatcher(
                            [
                                "grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer",
                                $"assertion={SampleJwtToken}",
                            ],
                            false,
                            MatchOperator.And
                        )
                    )
                    .UsingPost()
            )
            .ThenRespondWith(r =>
                r.WithStatusCode(200).WithBodyAsJson(SampleMaskinportenTokenResponse)
            );
    }

    protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
    {
        services.AddAltinnApiClients(
            _webHostEnvironment,
            new AltinnAuthenticationConfiguration()
            {
                MaskinportenUrl = new Uri(_server.Urls[0]),
                CertificatePrivateKey = "privateKey",
                IntegrationId = "integration",
                Scopes = ["test:read"],
            },
            new AltinnApiConfiguration()
            {
                AuthenticationUrl = new Uri($"{_server.Urls[0]}/authentication/api/v1/"),
                StorageUrl = new Uri($"{_server.Urls[0]}/storage/api/v1/"),
                EventUrl = new Uri($"{_server.Urls[0]}/events/api/v1/"),
                AppBaseUrl = new Uri(_server.Urls[0]),
            }
        );
        services.RemoveAll<IAltinnTokenProvider>();
        services.AddSingleton<IAltinnTokenProvider>(Substitute.For<IAltinnTokenProvider>());
    }

    protected override IEnumerable<TestAppSettings> GetTestAppSettings() => [];

    protected override ValueTask DisposeAsyncCore()
    {
        _server.Stop();
        _server.Dispose();
        return ValueTask.CompletedTask;
    }
}

file static class Extensions
{
    public static void AddMappings(
        this WireMockServer server,
        string filePath,
        Func<MappingModel, MappingModel>? mappingVisitor = null,
        WireMockOpenApiParserSettings? settings = null
    )
    {
        mappingVisitor ??= m => m;

        using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        server.AddOpenApiMappings(fileStream, mappingVisitor, settings);
    }
}
