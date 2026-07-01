using Arbeidstilsynet.Common.GeoNorge.Adresser.Models;
using Arbeidstilsynet.Common.GeoNorge.Extensions;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Unit;

public class AddressSearchExtensionsTests
{
    private IAddressSearch _addressSearch = Substitute.For<IAddressSearch>();

    [Fact]
    public async Task GetClosestAddress_MapsRequestCorrectly()
    {
        // Arrange
        var query = new PointSearchQuery()
        {
            Latitude = 60.0,
            Longitude = 10.0,
            RadiusInMeters = 1000,
        };

        // Act
        await _addressSearch.GetClosestAddress(query);

        // Assert
        await _addressSearch
            .Received(1)
            .SearchAddressesByPoint(query, new Pagination() { PageIndex = 0, PageSize = 1 });
    }

    [Fact]
    public async Task GetClosestAddress_MapsResultCorrectly()
    {
        // Arrange
        var query = new PointSearchQuery()
        {
            Latitude = 60.0,
            Longitude = 10.0,
            RadiusInMeters = 1000,
        };

        var returnedList = new OutputGeoPointList
        {
            Adresser =
            [
                new OutputGeoPoint { Adressenavn = "Testveien 1" },
                new OutputGeoPoint { Adressenavn = "Testveien 2" },
            ],
        };

        _addressSearch.SearchAddressesByPoint(default!).ReturnsForAnyArgs(returnedList);

        // Act
        var result = await _addressSearch.GetClosestAddress(query);

        // Assert
        result.ShouldBe(returnedList.Adresser.FirstOrDefault());
    }

    [Fact]
    public async Task QuickSearchLocation_MapsRequestCorrectly()
    {
        // Arrange
        var query = new TextSearchQuery()
        {
            SearchTerm = "Testveien",
            Adressenavn = "Testveien",
            Postnummer = "1234",
            Poststed = "Testby",
            Kommunenummer = "2510",
        };

        // Act
        await _addressSearch.QuickSearchLocation(query);

        // Assert
        await _addressSearch
            .Received(1)
            .SearchAddresses(query, new Pagination() { PageIndex = 0, PageSize = 1 });
    }

    [Fact]
    public async Task QuickSearchLocation_MapsResultCorrectly()
    {
        // Arrange
        var query = new TextSearchQuery()
        {
            SearchTerm = "Testveien",
            Adressenavn = "Testveien",
            Postnummer = "1234",
            Poststed = "Testby",
            Kommunenummer = "2510",
        };

        var returnedList = new OutputAdresseList
        {
            Adresser =
            [
                new OutputAdresse
                {
                    Representasjonspunkt = new GeomPoint { Lat = 60.0f, Lon = 10.0f },
                },
                new OutputAdresse
                {
                    Representasjonspunkt = new GeomPoint { Lat = 61.0f, Lon = 11.0f },
                },
            ],
        };

        _addressSearch.SearchAddresses(default!, default!).ReturnsForAnyArgs(returnedList);

        // Act
        var result = await _addressSearch.QuickSearchLocation(query);

        // Assert
        result.ShouldBe(returnedList.Adresser.FirstOrDefault()?.Representasjonspunkt);
    }
}
