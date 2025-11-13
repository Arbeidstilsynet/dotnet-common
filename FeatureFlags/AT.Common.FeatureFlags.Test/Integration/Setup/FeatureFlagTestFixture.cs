using Arbeidstilsynet.Common.FeatureFlags.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Unleash;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.FeatureFlags.Test.Integration.Setup;

public class FeatureFlagsTestFixture : TestBedFixture
{
    protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
    {
        services.AddFeatureFlags(
            new()
            {
                Url = "http://example.com",
                ApiKey = "etellerannet",
                Environment = "development",
            }
        );
        services.RemoveAll<IUnleash>();
        services.AddSingleton<IUnleash>(new FakeUnleash());
    }

    protected override IEnumerable<TestAppSettings> GetTestAppSettings() => [];

    protected override ValueTask DisposeAsyncCore()
    {
        return ValueTask.CompletedTask;
    }
}
