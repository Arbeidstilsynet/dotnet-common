using Arbeidstilsynet.Common.GeoNorge.DependencyInjection;
using AT.Common.GeoNorge.Test.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Logging;
using WireMock.Server;
using WireMock.Settings;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Integration.Setup;

public class GeoNorgeTestFixture : TestBedFixture
{
    private readonly WireMockServer _server;

    public GeoNorgeTestFixture()
    {
        _server = WireMockServer.Start(
            new WireMockServerSettings { Logger = new WireMockConsoleLogger() }
        );

        using var fileStream = File.Open(
            "Integration/TestData/openapi.json",
            FileMode.Open,
            FileAccess.Read
        );

        _server.AddOpenApiMappings(fileStream, m =>
        {
            
            
            return m;
        });
    }

    protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
    {
        services.AddGeoNorge(
            new GeoNorgeConfig() { BaseUrl = _server.Urls[0] }
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