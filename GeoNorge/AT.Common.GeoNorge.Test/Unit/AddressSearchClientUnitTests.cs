using Arbeidstilsynet.Common.GeoNorge.Adresser;
using Arbeidstilsynet.Common.GeoNorge.Adresser.Models;
using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using NSubstitute;
using Shouldly;
using PunktsokQueryParameters = Arbeidstilsynet.Common.GeoNorge.Adresser.Punktsok.PunktsokRequestBuilder.PunktsokRequestBuilderGetQueryParameters;
using SokQueryParameters = Arbeidstilsynet.Common.GeoNorge.Adresser.Sok.SokRequestBuilder.SokRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Unit;

public class AddressSearchClientUnitTests
{
    private readonly IRequestAdapter _requestAdapter = Substitute.For<IRequestAdapter>();
    private readonly AddressSearchClient _sut;

    public AddressSearchClientUnitTests()
    {
        _requestAdapter.BaseUrl = "https://ws.geonorge.no/adresser/v1";
        _requestAdapter
            .SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<OutputAdresseList>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new OutputAdresseList());
        _requestAdapter
            .SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<OutputGeoPointList>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new OutputGeoPointList());

        _sut = new AddressSearchClient(
            new AdresserClient(_requestAdapter),
            Substitute.For<ILogger<AddressSearchClient>>()
        );
    }

    [Fact]
    public async Task SearchAddresses_KoordsysNotSpecified_DefaultsUtkoordsysTo4326()
    {
        // Arrange
        var query = new SokQueryParameters { Sok = "Testveien" };

        // Act
        await _sut.SearchAddresses(query);

        // Assert
        query.Utkoordsys.ShouldBe(4326);
    }

    [Fact]
    public async Task SearchAddresses_UtkoordsysSpecified_KeepsCallerValue()
    {
        // Arrange
        var query = new SokQueryParameters { Sok = "Testveien", Utkoordsys = 25833 };

        // Act
        await _sut.SearchAddresses(query);

        // Assert
        query.Utkoordsys.ShouldBe(25833);
    }

    [Fact]
    public async Task SearchAddressesByPoint_KoordsysNotSpecified_DefaultsKoordsysAndUtkoordsysTo4326()
    {
        // Arrange
        var query = new PunktsokQueryParameters
        {
            Lat = 60.0f,
            Lon = 10.0f,
            Radius = 1000,
        };

        // Act
        await _sut.SearchAddressesByPoint(query);

        // Assert
        query.Koordsys.ShouldBe(4326);
        query.Utkoordsys.ShouldBe(4326);
    }

    [Fact]
    public async Task SearchAddressesByPoint_KoordsysSpecified_KeepsCallerValues()
    {
        // Arrange
        var query = new PunktsokQueryParameters
        {
            Lat = 60.0f,
            Lon = 10.0f,
            Radius = 1000,
            Koordsys = 25833,
            Utkoordsys = 25833,
        };

        // Act
        await _sut.SearchAddressesByPoint(query);

        // Assert
        query.Koordsys.ShouldBe(25833);
        query.Utkoordsys.ShouldBe(25833);
    }
}
