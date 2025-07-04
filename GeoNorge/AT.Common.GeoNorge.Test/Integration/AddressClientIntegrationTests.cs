using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Arbeidstilsynet.Common.GeoNorge.Test.Integration.Setup;
using Shouldly;
using WireMock.Pact.Models.V2;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.GeoNorge.Adapters.Test.Integration;

public class AddressClientIntegrationTests : TestBed<GeoNorgeTestFixture>
{
    private readonly IAddressSearch _sut;
    private readonly VerifySettings _verifySettings = new VerifySettings();

    public AddressClientIntegrationTests(
        ITestOutputHelper testOutputHelper,
        GeoNorgeTestFixture fixture
    )
        : base(testOutputHelper, fixture)
    {
        _sut = fixture.GetService<IAddressSearch>(testOutputHelper)!;

        _verifySettings.UseDirectory("TestData/Snapshots");
    }

    [Fact]
    public async Task SearchAddresses_ValidRequest_DeserializesResult()
    {
        // Act
        var result = await _sut.SearchAddresses(
            new TextSearchQuery() { SearchTerm = "Storgata 1, Oslo" }
        );

        // Assert
        result.ShouldNotBeNull();
        await Verify(result, _verifySettings);
    }

    [Fact]
    public async Task SearchAddressesByPoint_ValidRequest_DeserializesResult()
    {
        // Act
        var result = await _sut.SearchAddressesByPoint(
            new PointSearchQuery()
            {
                Latitude = 4.2,
                Longitude = 4.2,
                RadiusInMeters = 42,
            }
        );

        // Assert
        result.ShouldNotBeNull();
        await Verify(result, _verifySettings);
    }
}
