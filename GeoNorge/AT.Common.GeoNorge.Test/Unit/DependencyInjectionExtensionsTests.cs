using Arbeidstilsynet.Common.GeoNorge.DependencyInjection;
using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Unit;

public class DependencyInjectionExtensionsTests
{
    [Fact]
    public void AddGeoNorge_ApproximationEnabled_RegistersApproximateFylkeKommuneApi()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddGeoNorge(new GeoNorgeConfig { UseApproximateSvalbardAndJanMayen = true });
        using var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider
            .GetRequiredService<IFylkeKommuneApi>()
            .ShouldBeOfType<ApproximateSvalbardAndJanMayenFylkeKommuneApi>();
    }

    [Fact]
    public void AddGeoNorge_ApproximationDisabled_RegistersDefaultFylkeKommuneApi()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddGeoNorge(new GeoNorgeConfig { UseApproximateSvalbardAndJanMayen = false });
        using var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider
            .GetRequiredService<IFylkeKommuneApi>()
            .ShouldBeOfType<FylkeKommuneClient>();
    }
}
