using Arbeidstilsynet.Common.GeoNorge.Extensions;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Model.Response;
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

        var returnedElements = new List<Address>()
        {
            new() { Adressenavn = "Testveien 1" },
            new() { Adressenavn = "Testveien 2" },
        };

        var returnedPagination = new PaginationResult<Address>() { Elements = returnedElements };

        _addressSearch.SearchAddressesByPoint(default!).ReturnsForAnyArgs(returnedPagination);

        // Act
        var result = await _addressSearch.GetClosestAddress(query);

        // Assert

        result.ShouldBe(returnedPagination.Elements.FirstOrDefault());
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

        var returnedElements = new List<Address>()
        {
            new()
            {
                Location = new Location() { Latitude = 60.0, Longitude = 10.0 },
            },
            new()
            {
                Location = new Location() { Latitude = 61.0, Longitude = 11.0 },
            },
        };

        var returnedPagination = new PaginationResult<Address>() { Elements = returnedElements };

        _addressSearch.SearchAddresses(default!, default!).ReturnsForAnyArgs(returnedPagination);

        // Act
        var result = await _addressSearch.QuickSearchLocation(query);

        // Assert

        result.ShouldBe(returnedPagination.Elements.FirstOrDefault()?.Location);
    }
}
