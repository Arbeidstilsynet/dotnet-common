using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Arbeidstilsynet.Common.GeoNorge.KommuneInfo;
using Arbeidstilsynet.Common.GeoNorge.KommuneInfo.Models;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Microsoft.Kiota.Abstractions;
using NSubstitute;
using Shouldly;
using PunktQueryParameters = Arbeidstilsynet.Common.GeoNorge.KommuneInfo.Punkt.PunktRequestBuilder.PunktRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Unit;

public class FylkeKommuneClientUnitTests
{
    private readonly FylkeKommuneClient _sut = new FylkeKommuneClient(
        new KommuneInfoClient(Substitute.For<IRequestAdapter>())
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
            f.Fylkesnummer == "21" && f.Kommuner!.Single().Kommunenummer == "2100"
        );
        result.ShouldContain(f =>
            f.Fylkesnummer == "22" && f.Kommuner!.Single().Kommunenummer == "2211"
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
        result.Kommunenavn.ShouldBe(expectedKommunenavn);
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
            new PunktQueryParameters
            {
                Nord = latitude,
                Ost = longitude,
                Koordsys = 4326,
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
            new PunktQueryParameters
            {
                Nord = 59.91,
                Ost = 10.75,
                Koordsys = 4326,
            }
        );

        // Assert
        result.ShouldNotBeNull();
        result.Kommunenummer.ShouldBe("0301");
        _inner.GetKommuneByPointWasCalled.ShouldBeTrue();
    }

    [Theory]
    [InlineData(25833)] // UTM zone 33N (projected)
    [InlineData(null)] // no coordinate system specified
    public async Task GetKommuneByPoint_PointInsideBoundingBoxButNonGeographicKoordsys_UsesInnerApi(
        int? koordsys
    )
    {
        // Act
        var result = await _sut.GetKommuneByPoint(
            new PunktQueryParameters
            {
                Nord = 78.2232,
                Ost = 15.6469,
                Koordsys = koordsys,
            }
        );

        // Assert
        result.ShouldNotBeNull();
        result.Kommunenummer.ShouldBe("0301");
        _inner.GetKommuneByPointWasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task GetFylkeByNumber_NonSvalbardOrJanMayen_UsesInnerApi()
    {
        // Act
        var result = await _sut.GetFylkeByNumber("03");

        // Assert
        result.ShouldNotBeNull();
        result.Fylkesnavn.ShouldBe("Inner");
    }

    [Fact]
    public async Task GetKommuneByNumber_NonSvalbardOrJanMayen_UsesInnerApi()
    {
        // Act
        var result = await _sut.GetKommuneByNumber("0301");

        // Assert
        result.ShouldNotBeNull();
        result.Kommunenavn.ShouldBe("Inner");
    }

    [Fact]
    public async Task GetFylker_InnerAlreadyContainsSvalbardAndJanMayen_DoesNotDuplicate()
    {
        // Arrange
        var inner = Substitute.For<IFylkeKommuneApi>();
        inner
            .GetFylker()
            .Returns(
                Task.FromResult<IEnumerable<FylkerEnkel>>(
                    [
                        new FylkerEnkel { Fylkesnummer = "21", Fylkesnavn = "Inner Svalbard" },
                        new FylkerEnkel { Fylkesnummer = "22", Fylkesnavn = "Inner Jan Mayen" },
                    ]
                )
            );
        var sut = new ApproximateSvalbardAndJanMayenFylkeKommuneApi(inner);

        // Act
        var result = (await sut.GetFylker()).ToList();

        // Assert
        result.Count(f => f.Fylkesnummer == "21").ShouldBe(1);
        result.Count(f => f.Fylkesnummer == "22").ShouldBe(1);
        result.ShouldContain(f => f.Fylkesnummer == "21" && f.Fylkesnavn == "Inner Svalbard");
        result.ShouldContain(f => f.Fylkesnummer == "22" && f.Fylkesnavn == "Inner Jan Mayen");
    }

    [Fact]
    public async Task GetKommuner_InnerAlreadyContainsSvalbardAndJanMayen_DoesNotDuplicate()
    {
        // Arrange
        var inner = Substitute.For<IFylkeKommuneApi>();
        inner
            .GetKommuner()
            .Returns(
                Task.FromResult<IEnumerable<KomEnkelNorskNavn>>(
                    [
                        new KomEnkelNorskNavn
                        {
                            Kommunenummer = "2100",
                            Kommunenavn = "Inner Svalbard",
                        },
                        new KomEnkelNorskNavn
                        {
                            Kommunenummer = "2211",
                            Kommunenavn = "Inner Jan Mayen",
                        },
                    ]
                )
            );
        var sut = new ApproximateSvalbardAndJanMayenFylkeKommuneApi(inner);

        // Act
        var result = (await sut.GetKommuner()).ToList();

        // Assert
        result.Count(k => k.Kommunenummer == "2100").ShouldBe(1);
        result.Count(k => k.Kommunenummer == "2211").ShouldBe(1);
    }

    [Fact]
    public async Task GetFylkerFullInfo_InnerAlreadyContainsSvalbardAndJanMayen_DoesNotDuplicate()
    {
        // Arrange
        var inner = Substitute.For<IFylkeKommuneApi>();
        inner
            .GetFylkerFullInfo()
            .Returns(
                Task.FromResult<IEnumerable<FylkerKommunerFull>>(
                    [
                        new FylkerKommunerFull { Fylkesnummer = "21", Fylkesnavn = "Inner Svalbard" },
                        new FylkerKommunerFull { Fylkesnummer = "22", Fylkesnavn = "Inner Jan Mayen" },
                    ]
                )
            );
        var sut = new ApproximateSvalbardAndJanMayenFylkeKommuneApi(inner);

        // Act
        var result = (await sut.GetFylkerFullInfo()).ToList();

        // Assert
        result.Count(f => f.Fylkesnummer == "21").ShouldBe(1);
        result.Count(f => f.Fylkesnummer == "22").ShouldBe(1);
    }

    private class StubFylkeKommuneApi : IFylkeKommuneApi
    {
        public bool GetKommuneByPointWasCalled { get; private set; }

        public Task<IEnumerable<FylkerEnkel>> GetFylker()
        {
            return Task.FromResult<IEnumerable<FylkerEnkel>>([
                new FylkerEnkel { Fylkesnummer = "03", Fylkesnavn = "Oslo" },
            ]);
        }

        public Task<IEnumerable<KomEnkelNorskNavn>> GetKommuner()
        {
            return Task.FromResult<IEnumerable<KomEnkelNorskNavn>>([
                new KomEnkelNorskNavn { Kommunenummer = "0301", Kommunenavn = "Oslo" },
            ]);
        }

        public Task<IEnumerable<FylkerKommunerFull>> GetFylkerFullInfo()
        {
            return Task.FromResult<IEnumerable<FylkerKommunerFull>>([
                new FylkerKommunerFull
                {
                    Fylkesnummer = "03",
                    Fylkesnavn = "Oslo",
                    Kommuner =
                    [
                        new KomFull
                        {
                            Fylkesnummer = "03",
                            Kommunenummer = "0301",
                            Kommunenavn = "Oslo",
                        },
                    ],
                },
            ]);
        }

        public Task<FylkerKommunerEnkel?> GetFylkeByNumber(string fylkesnummer)
        {
            return Task.FromResult<FylkerKommunerEnkel?>(
                new FylkerKommunerEnkel { Fylkesnummer = fylkesnummer, Fylkesnavn = "Inner" }
            );
        }

        public Task<KomFull?> GetKommuneByNumber(string kommunenummer)
        {
            return Task.FromResult<KomFull?>(
                new KomFull
                {
                    Fylkesnummer = "03",
                    Kommunenummer = kommunenummer,
                    Kommunenavn = "Inner",
                }
            );
        }

        public Task<KommuneFylkeEnkel?> GetKommuneByPoint(PunktQueryParameters queryParameters)
        {
            GetKommuneByPointWasCalled = true;

            return Task.FromResult<KommuneFylkeEnkel?>(
                new KommuneFylkeEnkel
                {
                    Fylkesnummer = "03",
                    Fylkesnavn = "Oslo",
                    Kommunenummer = "0301",
                    Kommunenavn = "Oslo",
                }
            );
        }
    }
}
