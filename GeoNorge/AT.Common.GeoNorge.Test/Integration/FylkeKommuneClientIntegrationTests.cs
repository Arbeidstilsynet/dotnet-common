using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Arbeidstilsynet.Common.GeoNorge.Test.Integration.Setup;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.GeoNorge.Adapters.Test.Integration;

public class FylkeKommuneClientIntegrationTests : TestBed<GeoNorgeTestFixture>
{
    private readonly IFylkeKommuneApi _sut;
    private readonly VerifySettings _verifySettings = new VerifySettings();

    public FylkeKommuneClientIntegrationTests(
        ITestOutputHelper testOutputHelper,
        GeoNorgeTestFixture fixture
    )
        : base(testOutputHelper, fixture)
    {
        _sut = fixture.GetService<IFylkeKommuneApi>(testOutputHelper)!;

        _verifySettings.UseDirectory("TestData/Snapshots");
    }
    
    [Fact]
    public async Task GetFylker_ValidRequest_DeserializesResult()
    {
        // Act
        var result = await _sut.GetFylker();

        // Assert
        result.ShouldNotBeNull();
        await Verify(result, _verifySettings);
    }
    
    [Fact]
    public async Task GetKommuner_ValidRequest_DeserializesResult()
    {
        // Act
        var result = await _sut.GetKommuner();

        // Assert
        result.ShouldNotBeNull();
        await Verify(result, _verifySettings);
    }
    
    [Fact]
    public async Task GetFullFylkeDetails_ValidRequest_DeserializesResult()
    {
        // Act
        var result = await _sut.GetFylkerFullInfo();

        // Assert
        result.ShouldNotBeNull();
        await Verify(result, _verifySettings);
    }
    
    [Fact]
    public async Task GetFylkeByNumber_ValidRequest_DeserializesResult()
    {
        // Act
        var result = await _sut.GetFylkeByNumber("03");

        // Assert
        result.ShouldNotBeNull();
        await Verify(result, _verifySettings);
    }
    
    [Fact]
    public async Task GetKommuneByNumber_ValidRequest_DeserializesResult()
    {
        // Act
        var result = await _sut.GetKommuneByNumber("0301");

        // Assert
        result.ShouldNotBeNull();
        await Verify(result, _verifySettings);
    }
    
    [Fact]
    public async Task GetKommuneByPoint_ValidRequest_DeserializesResult()
    {
        // Act
        var result = await _sut.GetKommuneByPoint(
            new PointQuery() { Latitude = 4.2, Longitude = 4.2, Epsg = 42 }
        );

        // Assert
        result.ShouldNotBeNull();
        await Verify(result, _verifySettings);
    }
}