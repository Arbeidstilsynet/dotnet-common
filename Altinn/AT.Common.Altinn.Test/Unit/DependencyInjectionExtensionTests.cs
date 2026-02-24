using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class DependencyInjectionExtensionTests
{
    private readonly IWebHostEnvironment _production = Substitute.For<IWebHostEnvironment>();
    private readonly IWebHostEnvironment _develpment = Substitute.For<IWebHostEnvironment>();
    private readonly IWebHostEnvironment _staging = Substitute.For<IWebHostEnvironment>();

    private readonly VerifySettings _verifySettings = new();

    private readonly MaskinportenConfiguration _maskinportenConfiguration = new()
    {
        Scopes = ["scope1", "scope2"],
        CertificatePrivateKey = "some-private-key",
        CertificateChain = "some-certificate-chain",
        IntegrationId = "some-integration-id",
    };

    public DependencyInjectionExtensionTests()
    {
        _production.EnvironmentName.Returns(Environments.Production);
        _develpment.EnvironmentName.Returns(Environments.Development);
        _staging.EnvironmentName.Returns(Environments.Staging);

        _verifySettings.UseDirectory("Snapshots");
    }

    [Fact]
    public async Task AddAltinnClient_MergesPartialConfigurationCorrectly()
    {
        //arrange
        var services = new ServiceCollection();

        var partialConfiguration = new AltinnConfiguration
        {
            OrgId = "my-org-id",
            // These properties can be null in a real situation be cause they're resolved from configuration
            AuthenticationUrl = null!,
            StorageUrl = null!,
            EventUrl = null!,
            AppBaseUrl = null!,
        };

        //act
        services.AddAltinnAdapter(_production, _maskinportenConfiguration, partialConfiguration);
        //assert
        var serviceProvider = services.BuildServiceProvider();
        var result = serviceProvider.GetRequiredService<IOptions<AltinnConfiguration>>();

        await Verify(result, _verifySettings);
    }

    [Fact]
    public async Task AddAltinnClient_UsesExpectedAltinnConfiguration_InProduction()
    {
        //arrange
        var services = new ServiceCollection();

        //act
        services.AddAltinnAdapter(_production, _maskinportenConfiguration);
        //assert
        var serviceProvider = services.BuildServiceProvider();
        var result = serviceProvider.GetRequiredService<IOptions<AltinnConfiguration>>();

        await Verify(result, _verifySettings);
    }

    [Fact]
    public async Task AddAltinnClient_UsesExpectedAltinnConfiguration_InDevelopment()
    {
        //arrange
        var services = new ServiceCollection();
        //act
        services.AddAltinnAdapter(_develpment, _maskinportenConfiguration);
        //assert
        var serviceProvider = services.BuildServiceProvider();
        var result = serviceProvider.GetRequiredService<IOptions<AltinnConfiguration>>();
        await Verify(result, _verifySettings);
    }

    [Fact]
    public async Task AddAltinnClient_UsesExpectedAltinnConfiguration_InStaging()
    {
        //arrange
        var services = new ServiceCollection();
        //act
        services.AddAltinnAdapter(_staging, _maskinportenConfiguration);
        //assert
        var serviceProvider = services.BuildServiceProvider();
        var result = serviceProvider.GetRequiredService<IOptions<AltinnConfiguration>>();
        await Verify(result, _verifySettings);
    }
}
