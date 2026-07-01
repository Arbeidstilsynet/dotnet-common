using Arbeidstilsynet.Common.GeoNorge.Ports;
using Arbeidstilsynet.Common.GeoNorge.Test.Integration.Setup;
using Shouldly;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.GeoNorge.Adapters.Test.Integration;

public class FylkeKommuneClientIntegrationTests : TestBed<GeoNorgeTestFixture>
{
    private readonly IFylkeKommuneApi _sut;

    public FylkeKommuneClientIntegrationTests(
        ITestOutputHelper testOutputHelper,
        GeoNorgeTestFixture fixture
    )
        : base(testOutputHelper, fixture)
    {
        _sut = fixture.GetService<IFylkeKommuneApi>(testOutputHelper)!;
    }

    [Fact]
    public async Task GetFylker_ValidRequest_MapsToDomainResult()
    {
        // Act
        var result = (await _sut.GetFylker()).ToList();

        // Assert
        result.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetKommuner_ValidRequest_MapsToDomainResult()
    {
        // Act
        var result = (await _sut.GetKommuner()).ToList();

        // Assert
        result.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetFylkerFullInfo_ValidRequest_MapsToDomainResult()
    {
        // Act
        var result = (await _sut.GetFylkerFullInfo()).ToList();

        // Assert
        result.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetFylkeByNumber_ValidRequest_MapsToDomainResult()
    {
        // Act
        var result = await _sut.GetFylkeByNumber("03");

        // Assert
        result.ShouldNotBeNull();
        result.Fylkesnummer.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetKommuneByNumber_ValidRequest_MapsToDomainResult()
    {
        // Act
        var result = await _sut.GetKommuneByNumber("0301");

        // Assert
        result.ShouldNotBeNull();
        result.Kommunenummer.ShouldNotBeNull();
    }
}
