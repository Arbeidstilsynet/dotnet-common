using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Unit;

public class GeoNorgeClientUnitTests
{
    private readonly GeoNorgeClient _sut = new (
        Substitute.For<IHttpClientFactory>(),
        Substitute.For<ILogger<GeoNorgeClient>>()
        );
    
    [Fact]
    public void SearchAddressesByPoint_RequestIsInvalid_ThrowsArgumentException()
    {
        // Arrange
        var act = () => _sut.SearchAddressesByPoint(new PointSearchQuery()
        {
            Latitude = 60.0,
            Longitude = 10.0,
            RadiusInMeters = 0
        });
        
        // Act & Assert
        act.ShouldThrow<ArgumentException>();
    }
}