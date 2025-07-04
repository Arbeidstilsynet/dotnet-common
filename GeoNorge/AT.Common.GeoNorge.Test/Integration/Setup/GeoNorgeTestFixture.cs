using Arbeidstilsynet.Common.GeoNorge.DependencyInjection;
using Arbeidstilsynet.Common.TestExtensions.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Admin.Mappings;
using WireMock.Net.OpenApiParser.Settings;
using WireMock.Server;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Integration.Setup;

public class GeoNorgeTestFixture : TestBedFixture
{
    private readonly WireMockServer _server;

    public GeoNorgeTestFixture()
    {
        _server = WireMockServer.Start();
        _server.AddMappings("Integration/TestData/adresser-openapi.json");
        _server.AddMappings(
            "Integration/TestData/kommuner-openapi.json",
            KommuneFylkeMappingVisitor,
            new WireMockOpenApiParserSettings()
            {
                NumberOfArrayItems = 2, // Important because coordinates are arrays with 2 items (lat, lon)
            }
        );
    }

    protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
    {
        services.AddGeoNorge(new GeoNorgeConfig() { BaseUrl = _server.Urls[0] });
    }

    protected override IEnumerable<TestAppSettings> GetTestAppSettings() => [];

    protected override ValueTask DisposeAsyncCore()
    {
        _server.Stop();
        _server.Dispose();
        return ValueTask.CompletedTask;
    }

    private MappingModel KommuneFylkeMappingVisitor(MappingModel mapping)
    {
        mapping.Request.Path = mapping.Request.Path switch
        {
            string path and "/kommuneinfo/v1/fylker/example-string" => path.Replace(
                "example-string",
                "03"
            ),
            string path2 and "/kommuneinfo/v1/kommuner/example-string" => path2.Replace(
                "example-string",
                "0301"
            ),
            _ => mapping.Request.Path,
        };

        return mapping;
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
