using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Unit;

public class AddressSearchClientUnitTests
{
    private readonly AddressSearchClient _sut = new(
        Substitute.For<IHttpClientFactory>(),
        Substitute.For<ILogger<AddressSearchClient>>()
    );

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SearchAddressesByPoint_RequestIsInvalid_ThrowsArgumentException(
        double invalidRadius
    )
    {
        // Arrange
        var act = () =>
            _sut.SearchAddressesByPoint(
                new PointSearchQuery()
                {
                    Latitude = 60.0,
                    Longitude = 10.0,
                    RadiusInMeters = invalidRadius,
                }
            );

        // Act & Assert
        act.ShouldThrow<ArgumentException>();
    }
}
