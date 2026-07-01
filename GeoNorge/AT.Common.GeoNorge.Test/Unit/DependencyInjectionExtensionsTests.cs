using Arbeidstilsynet.Common.GeoNorge.Adresser;
using Arbeidstilsynet.Common.GeoNorge.DependencyInjection;
using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Arbeidstilsynet.Common.GeoNorge.KommuneInfo;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Unit;

public class DependencyInjectionExtensionsTests
{
    [Fact]
    public void AddGeoNorge_RegistersAdresserClient()
    {
        using var scope = BuildScope();

        scope.ServiceProvider.GetService<AdresserClient>().ShouldNotBeNull();
    }

    [Fact]
    public void AddGeoNorge_RegistersKommuneInfoClient()
    {
        using var scope = BuildScope();

        scope.ServiceProvider.GetService<KommuneInfoClient>().ShouldNotBeNull();
    }

    [Fact]
    public void AddGeoNorge_RegistersAddressSearchPort()
    {
        using var scope = BuildScope();

        scope.ServiceProvider.GetService<IAddressSearch>().ShouldNotBeNull();
    }

    [Fact]
    public void AddGeoNorge_RegistersFylkeKommunePort()
    {
        using var scope = BuildScope();

        scope.ServiceProvider.GetService<IFylkeKommuneApi>().ShouldNotBeNull();
    }

    [Fact]
    public void AddGeoNorge_WithoutApproximation_DoesNotDecorateFylkeKommuneApi()
    {
        using var scope = BuildScope(new GeoNorgeConfig { UseApproximateSvalbardAndJanMayen = false });

        scope
            .ServiceProvider.GetRequiredService<IFylkeKommuneApi>()
            .ShouldBeOfType<FylkeKommuneClient>();
    }

    [Fact]
    public void AddGeoNorge_WithApproximation_DecoratesFylkeKommuneApi()
    {
        using var scope = BuildScope(new GeoNorgeConfig { UseApproximateSvalbardAndJanMayen = true });

        scope
            .ServiceProvider.GetRequiredService<IFylkeKommuneApi>()
            .ShouldBeOfType<ApproximateSvalbardAndJanMayenFylkeKommuneApi>();
    }

    private static IServiceScope BuildScope(GeoNorgeConfig? config = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGeoNorge(config);

        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.CreateScope();
    }
}
