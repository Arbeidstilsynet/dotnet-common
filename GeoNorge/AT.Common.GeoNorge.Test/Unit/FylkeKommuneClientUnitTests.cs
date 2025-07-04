using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Unit;

public class FylkeKommuneClientUnitTests
{
    private readonly FylkeKommuneClient _sut = new FylkeKommuneClient(Substitute.For<IHttpClientFactory>(),
        Substitute.For<ILogger<FylkeKommuneClient>>());
    
    
    [Theory]
    [InlineData]
    public void GetFylke_ValidRequest_DeserializesResult(string invalidFylkenummer)
    {
        // Arrange
        var act = () => _sut.GetFylkeByNumber();

        // Act & Assert
        act.ShouldNotThrow();
    }
}
