using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.TestExtensions.Extensions;
using GraphQL.Utilities;
using GraphQL.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OpenTelemetry.Trace;
using WireMock.Admin.Mappings;
using WireMock.Net.OpenApiParser.Settings;
using WireMock.Net.OpenApiParser.Types;
using WireMock.Server;
using WireMock.Settings;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Altinn.Test.Setup;

public class AltinnApiTestFixture : TestBedFixture
{
    private readonly WireMockServer _server;

    private readonly IAltinnTokenProvider _tokenProvider = Substitute.For<IAltinnTokenProvider>();

    private readonly IWebHostEnvironment _webHostEnvironment =
        Substitute.For<IWebHostEnvironment>();

    public AltinnApiTestFixture()
    {
        _server = WireMockServer.Start(
            new WireMockServerSettings
            {
                AllowCSharpCodeMatcher = true,
                StartAdminInterface = true,
                ReadStaticMappings = true,
                WatchStaticMappings = false,
                WatchStaticMappingsInSubdirectories = false,
                SaveUnmatchedRequests = true,
            }
        );
        _server.AddMappings(
            "Unit/TestData/openapi/altinn-platform-events-v1.json",
            null,
            new WireMockOpenApiParserSettings { DynamicExamples = false }
        );
        _server.AddMappings(
            "Unit/TestData/openapi/altinn-platform-storage-v1.json",
            null,
            new WireMockOpenApiParserSettings
            {
                ExampleValues = new DynamicDataGeneration(),
                PathPatternToUse = ExampleValueType.Value,
            }
        );
    }

    protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
    {
        services.AddAltinnApiClients(
            _webHostEnvironment,
            _tokenProvider,
            new AltinnApiConfiguration()
            {
                StorageUrl = new Uri($"{_server.Urls[0]}/storage/api/v1/"),
                EventUrl = new Uri($"{_server.Urls[0]}/events/api/v1/"),
                AppBaseUrl = new Uri(_server.Urls[0]),
            }
        );
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
