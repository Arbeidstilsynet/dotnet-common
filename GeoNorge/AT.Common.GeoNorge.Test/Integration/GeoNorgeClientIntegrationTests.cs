using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Test.Integration.Setup;
using WireMock.Pact.Models.V2;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.GeoNorge.Adapters.Test.Integration;

public class GeoNorgeClientIntegrationTests : TestBed<GeoNorgeTestFixture>
{
    private readonly IGeoNorge _sut;
    private readonly VerifySettings _verifySettings = new VerifySettings();
    
    public GeoNorgeClientIntegrationTests(ITestOutputHelper testOutputHelper, GeoNorgeTestFixture fixture) : base(testOutputHelper, fixture)
    {
        _sut = fixture.GetService<IGeoNorge>(testOutputHelper)!;

        _verifySettings.UseDirectory("TestData/Snapshots");
    }
    
    [Fact]
    public async Task SearchAddresses_ValidRequest_ReturnsLocation()
    {
        // Act
        var result = await _sut.SearchAddresses(new TextSearchQuery()
        {
            SearchTerm = "Storgata 1, Oslo",
        });

        // Assert
        await Verify(result, _verifySettings);
    }
    
    [Fact]
    public async Task SearchAddressesByPoint_ValidRequest_ReturnsAddress()
    {
        // Act
        var result = await _sut.SearchAddressesByPoint(new PointSearchQuery()
        {
            Point = new Location()
            {
                Latitude = 4.2,
                Longitude = 4.2
            },
            RadiusInMeters = 42
        });

        // Assert
        await Verify(result, _verifySettings);
    }
    
}