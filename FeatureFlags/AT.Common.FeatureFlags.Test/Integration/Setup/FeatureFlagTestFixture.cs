using Arbeidstilsynet.Common.FeatureFlags.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Unleash;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.FeatureFlags.Test.Integration.Setup;

public class FeatureFlagsTestFixture : TestBedFixture
{
    protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
    {
        var fakeUnleash = new FakeUnleash();
        services.AddSingleton<IUnleash>(fakeUnleash);
        services.AddFeatureFlags(Substitute.For<IWebHostEnvironment>(), null);
    }

    protected override IEnumerable<TestAppSettings> GetTestAppSettings() => [];

    protected override ValueTask DisposeAsyncCore()
    {
        return ValueTask.CompletedTask;
    }
}
