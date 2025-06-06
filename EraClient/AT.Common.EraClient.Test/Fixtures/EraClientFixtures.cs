using Arbeidstilsynet.Common.EraClient.DependencyInjection;
using Arbeidstilsynet.Common.EraClient.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using WireMock.RequestBuilders;
using WireMock.Server;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.EraClient.Test.Fixtures;

/// <summary>
/// Fixture for testing EraClient with adding era clients via DI.
/// </summary>
public class EraClientFixture : TestBedFixture
{
    internal WireMockServer WireMockServer = WireMockServer.Start();

    private readonly IHostEnvironment _hostEnvironment = Substitute.For<IHostEnvironment>();

    protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
    {
        services.AddSingleton(_hostEnvironment);
        services.AddEraAdapter(options =>
        {
            options.AuthenticationUrl = WireMockServer.Urls[0];
            options.EraAsbestUrl = WireMockServer.Urls[0];
        });
    }

    protected override ValueTask DisposeAsyncCore() => new();

    protected override IEnumerable<TestAppSettings> GetTestAppSettings()
    {
        yield return new() { Filename = "appsettings.json", IsOptional = true };
    }
}
