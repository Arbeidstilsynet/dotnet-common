using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Model.Response;
using Arbeidstilsynet.Common.GeoNorge.Ports;
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

public class ApproximateSvalbardAndJanMayenFylkeKommuneApiTests
{
    private readonly StubFylkeKommuneApi _inner = new();
    private readonly ApproximateSvalbardAndJanMayenFylkeKommuneApi _sut;

    public ApproximateSvalbardAndJanMayenFylkeKommuneApiTests()
    {
        _sut = new ApproximateSvalbardAndJanMayenFylkeKommuneApi(_inner);
    }

    [Fact]
    public async Task GetFylker_AddsSvalbardAndJanMayen()
    {
        // Act
        var result = (await _sut.GetFylker()).ToList();

        // Assert
        result.ShouldContain(f => f.Fylkesnummer == "21" && f.Fylkesnavn == "Svalbard");
        result.ShouldContain(f => f.Fylkesnummer == "22" && f.Fylkesnavn == "Jan Mayen");
    }

    [Fact]
    public async Task GetKommuner_AddsSvalbardAndJanMayen()
    {
        // Act
        var result = (await _sut.GetKommuner()).ToList();

        // Assert
        result.ShouldContain(k => k.Kommunenummer == "2100" && k.Kommunenavn == "Svalbard");
        result.ShouldContain(k => k.Kommunenummer == "2211" && k.Kommunenavn == "Jan Mayen");
    }

    [Fact]
    public async Task GetFylkerFullInfo_AddsSvalbardAndJanMayen()
    {
        // Act
        var result = (await _sut.GetFylkerFullInfo()).ToList();

        // Assert
        result.ShouldContain(f =>
            f.Fylke.Fylkesnummer == "21" && f.Kommuner.Single().Kommune.Kommunenummer == "2100"
        );
        result.ShouldContain(f =>
            f.Fylke.Fylkesnummer == "22" && f.Kommuner.Single().Kommune.Kommunenummer == "2211"
        );
    }

    [Theory]
    [InlineData("21", "Svalbard")]
    [InlineData("22", "Jan Mayen")]
    public async Task GetFylkeByNumber_ReturnsApproximatedFylke(
        string fylkesnummer,
        string expectedFylkesnavn
    )
    {
        // Act
        var result = await _sut.GetFylkeByNumber(fylkesnummer);

        // Assert
        result.ShouldNotBeNull();
        result.Fylkesnavn.ShouldBe(expectedFylkesnavn);
    }

    [Theory]
    [InlineData("2100", "Svalbard")]
    [InlineData("2211", "Jan Mayen")]
    public async Task GetKommuneByNumber_ReturnsApproximatedKommune(
        string kommunenummer,
        string expectedKommunenavn
    )
    {
        // Act
        var result = await _sut.GetKommuneByNumber(kommunenummer);

        // Assert
        result.ShouldNotBeNull();
        result.Kommune.Kommunenavn.ShouldBe(expectedKommunenavn);
    }

    [Theory]
    [InlineData(78.2232, 15.6469, "2100")]
    [InlineData(74.5036, 19.0017, "2100")] // Bjørnøya
    [InlineData(76.5097, 25.0111, "2100")] // Hopen
    [InlineData(70.9821, -8.5337, "2211")]
    public async Task GetKommuneByPoint_PointInsideBoundingBox_ReturnsApproximatedKommune(
        double latitude,
        double longitude,
        string expectedKommunenummer
    )
    {
        // Act
        var result = await _sut.GetKommuneByPoint(
            new PointQuery
            {
                Latitude = latitude,
                Longitude = longitude,
            }
        );

        // Assert
        result.ShouldNotBeNull();
        result.Kommunenummer.ShouldBe(expectedKommunenummer);
        _inner.GetKommuneByPointWasCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task GetKommuneByPoint_PointOutsideBoundingBoxes_UsesInnerApi()
    {
        // Act
        var result = await _sut.GetKommuneByPoint(
            new PointQuery
            {
                Latitude = 59.91,
                Longitude = 10.75,
            }
        );

        // Assert
        result.ShouldNotBeNull();
        result.Kommunenummer.ShouldBe("0301");
        _inner.GetKommuneByPointWasCalled.ShouldBeTrue();
    }

    private class StubFylkeKommuneApi : IFylkeKommuneApi
    {
        public bool GetKommuneByPointWasCalled { get; private set; }

        public Task<IEnumerable<Fylke>> GetFylker()
        {
            return Task.FromResult<IEnumerable<Fylke>>(
                [
                    new Fylke
                    {
                        Fylkesnummer = "03",
                        Fylkesnavn = "Oslo",
                    },
                ]
            );
        }

        public Task<IEnumerable<Kommune>> GetKommuner()
        {
            return Task.FromResult<IEnumerable<Kommune>>(
                [
                    new Kommune
                    {
                        Kommunenummer = "0301",
                        Kommunenavn = "Oslo",
                    },
                ]
            );
        }

        public Task<IEnumerable<FylkeFullInfo>> GetFylkerFullInfo()
        {
            return Task.FromResult<IEnumerable<FylkeFullInfo>>(
                [
                    new FylkeFullInfo
                    {
                        Fylke = new Fylke
                        {
                            Fylkesnummer = "03",
                            Fylkesnavn = "Oslo",
                        },
                        Kommuner =
                        [
                            new KommuneFullInfo
                            {
                                Fylkesnummer = "03",
                                Kommune = new Kommune
                                {
                                    Kommunenummer = "0301",
                                    Kommunenavn = "Oslo",
                                },
                            },
                        ],
                    },
                ]
            );
        }

        public Task<Fylke?> GetFylkeByNumber(string fylkesnummer)
        {
            return Task.FromResult<Fylke?>(
                new Fylke
                {
                    Fylkesnummer = fylkesnummer,
                    Fylkesnavn = "Inner",
                }
            );
        }

        public Task<KommuneFullInfo?> GetKommuneByNumber(string kommunenummer)
        {
            return Task.FromResult<KommuneFullInfo?>(
                new KommuneFullInfo
                {
                    Fylkesnummer = "03",
                    Kommune = new Kommune
                    {
                        Kommunenummer = kommunenummer,
                        Kommunenavn = "Inner",
                    },
                }
            );
        }

        public Task<Kommune?> GetKommuneByPoint(PointQuery query)
        {
            GetKommuneByPointWasCalled = true;

            return Task.FromResult<Kommune?>(
                new Kommune
                {
                    Kommunenummer = "0301",
                    Kommunenavn = "Oslo",
                }
            );
        }
    }
}
