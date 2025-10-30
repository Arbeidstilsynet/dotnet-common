using Arbeidstilsynet.Common.FeatureFlags.DependencyInjection;
using NSubstitute;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.FeatureFlags.Test.Integration.Setup;

public class FeatureFlagsTestFixture : TestBedFixture
{
  protected override void AddServices(IServiceCollection services, IConfiguration? configuration)
  {
    services.AddFeatureFlags(Substitute.For<IWebHostEnvironment>(), null);
  }

  protected override IEnumerable<TestAppSettings> GetTestAppSettings() => [];
  protected override ValueTask DisposeAsyncCore()
  {
    return ValueTask.CompletedTask;
  }
}