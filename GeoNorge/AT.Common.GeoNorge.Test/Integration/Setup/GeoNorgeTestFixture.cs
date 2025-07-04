using Arbeidstilsynet.Common.GeoNorge.DependencyInjection;
using Arbeidstilsynet.Common.TestExtensions.Extensions;
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
        _server = WireMockServer.Start();
        _server.AddMappings("Integration/TestData/adresser-openapi.json");
        _server.AddMappings("Integration/TestData/kommuner-openapi.json");
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
}

file static class Extensions
{
    public static void AddMappings(this WireMockServer server, string filePath)
    {
        using var fileStream = File.Open(
            filePath,
            FileMode.Open,
            FileAccess.Read
        );

        server.AddOpenApiMappings(fileStream);
    }
}