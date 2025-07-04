using Arbeidstilsynet.Common.GeoNorge.Implementation;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Unit;

public class FylkeKommuneClientUnitTests
{
    private readonly FylkeKommuneClient _sut = new FylkeKommuneClient(
        Substitute.For<IHttpClientFactory>()
    );

    [Theory]
    [InlineData("no")]
    [InlineData("1")]
    [InlineData("123")]
    public void GetFylkeByNumber_InvalidFylkenummer_ThrowsArgumentException(
        string invalidFylkenummer
    )
    {
        // Arrange
        var act = () => _sut.GetFylkeByNumber(invalidFylkenummer);

        // Act & Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData("nope")]
    [InlineData("123")]
    [InlineData("12345")]
    public void GetKommuneByNumber_InvalidKommuneId_ThrowsArgumentException(
        string invalidKommunenummer
    )
    {
        // Arrange
        var act = () => _sut.GetKommuneByNumber(invalidKommunenummer);

        // Act & Assert
        act.ShouldThrow<ArgumentException>();
    }
}
