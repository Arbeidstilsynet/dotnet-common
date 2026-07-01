using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Arbeidstilsynet.Common.GeoNorge.Test.Integration.Setup;
using Shouldly;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.GeoNorge.Adapters.Test.Integration;

public class AddressClientIntegrationTests : TestBed<GeoNorgeTestFixture>
{
    private readonly IAddressSearch _sut;

    public AddressClientIntegrationTests(
        ITestOutputHelper testOutputHelper,
        GeoNorgeTestFixture fixture
    )
        : base(testOutputHelper, fixture)
    {
        _sut = fixture.GetService<IAddressSearch>(testOutputHelper)!;
    }

    [Fact]
    public async Task SearchAddresses_ValidRequest_MapsToDomainResult()
    {
        // Act
        var result = await _sut.SearchAddresses(new TextSearchQuery { SearchTerm = "Storgata 1" });

        // Assert
        result.ShouldNotBeNull();
        result.Metadata.ShouldNotBeNull();
        result.Adresser.ShouldNotBeNull();
        result.Adresser.ShouldNotBeEmpty();

        var address = result.Adresser.First();
        address.Adressenavn.ShouldNotBeNull();
        address.Representasjonspunkt.ShouldNotBeNull();
    }
}
