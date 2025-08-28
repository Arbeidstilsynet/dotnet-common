using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;
using GraphQL.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test.Integration.Fixture;

public class EnhetsregisterTenorTestFixture : TestBedFixture
{
    private readonly IWebHostEnvironment WebHostMock = Substitute.For<IWebHostEnvironment>();

    protected override void AddServices(
        IServiceCollection services,
        global::Microsoft.Extensions.Configuration.IConfiguration? configuration
    )
    {
        WebHostMock.EnvironmentName.Returns("Development");
        services.AddEnhetsregisteret(
            WebHostMock,
            new EnhetsregisteretConfig { BrregApiBaseUrl = null }
        );
    }

    protected override ValueTask DisposeAsyncCore() => new();

    protected override IEnumerable<TestAppSettings> GetTestAppSettings()
    {
        yield return new() { Filename = "appsettings.json", IsOptional = true };
    }
}
