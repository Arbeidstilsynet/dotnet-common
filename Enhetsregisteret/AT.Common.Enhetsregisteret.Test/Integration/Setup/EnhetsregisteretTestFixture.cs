using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;
using Arbeidstilsynet.Common.TestExtensions.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using WireMock.Logging;
using WireMock.Server;
using WireMock.Settings;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test.Integration.Setup;

public class EnhetsregisteretTestFixture : TestBedFixture
{
    private readonly WireMockServer _server;

    public EnhetsregisteretTestFixture()
    {
        _server = WireMockServer.Start(
            new WireMockServerSettings { Logger = new WireMockConsoleLogger() }
        );

        var fileStream = File.Open(
            "Integration/TestData/openapi.json",
            FileMode.Open,
            FileAccess.Read
        );

        _server.AddOpenApiMappings(fileStream);
    }

    protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
    {
        services.AddEnhetsregisteret(
            Substitute.For<IWebHostEnvironment>(),
            new EnhetsregisteretConfig()
            {
                BrregApiBaseUrl = _server.Urls[0],
                CacheOptions = new CacheOptions { Disabled = false },
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
