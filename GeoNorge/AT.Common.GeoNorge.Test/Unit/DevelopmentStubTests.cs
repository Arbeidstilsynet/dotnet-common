using Arbeidstilsynet.Common.GeoNorge.Extensions;
using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Microsoft.Kiota.Abstractions.Serialization;
using Shouldly;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Unit;

public class DevelopmentStubTests
{
    [Theory]
    [InlineData("0001", null, null, null, null, true)]
    [InlineData(null, "0301", null, null, null, true)]
    [InlineData(null, null, 1, null, null, true)]
    [InlineData(null, null, null, 1, null, true)]
    [InlineData("0001", null, null, null, "eksempelveien", true)]
    [InlineData(null, "0301", 1, 1, null, true)]
    [InlineData("0001", "0301", 1, 1, "eksempelveien", true)]
    [InlineData("0002", null, null, null, null, false)]
    [InlineData(null, "0302", null, null, null, false)]
    [InlineData(null, null, 2, null, null, false)]
    [InlineData(null, null, null, 2, null, false)]
    [InlineData("0002", "0301", 1, 1, "eksempelveien", false)]
    [InlineData("0001", "0302", 1, 1, "eksempelveien", false)]
    [InlineData("0001", "0301", 2, 1, "eksempelveien", false)]
    [InlineData("0001", "0301", 1, 2, "eksempelveien", false)]
    public async Task AddressFiltersMatchSampleAndPreservePaginationMetadata(
        string? postcode,
        string? municipality,
        int? farm,
        int? use,
        string? search,
        bool matches
    )
    {
        var stub = new AddressSearchStub();
        foreach (var page in new[] { 0, 1 })
        {
            var result = await stub.SearchAddresses(
                new()
                {
                    Postnummer = postcode,
                    Kommunenummer = municipality,
                    Gardsnummer = farm,
                    Bruksnummer = use,
                    Sok = search,
                    Side = page,
                    TreffPerSide = 1,
                }
            );
            var hasResult = matches && page == 0;
            result!.Adresser!.Count.ShouldBe(hasResult ? 1 : 0);
            result.Metadata!.TotaltAntallTreff.ShouldBe(matches ? 1 : 0);
            result.Metadata.Side.ShouldBe(page);
            result.Metadata.TreffPerSide.ShouldBe(1);
            result.Metadata.ViserFra.ShouldBe(hasResult ? 0 : null);
            result.Metadata.ViserTil.ShouldBe(hasResult ? 0 : null);
        }
    }

    [Fact]
    public async Task AddressSearchSupportsSampleAndEmptyResultsAndFreshModels()
    {
        var stub = new AddressSearchStub();
        var sample = await stub.SearchAddresses(new() { Sok = "eksempelveien" });
        sample!.Adresser.ShouldHaveSingleItem().Adressetekst.ShouldBe("Eksempelveien 1");
        sample.Metadata!.TotaltAntallTreff.ShouldBe(1);
        sample.Adresser[0].Adressetekst = "modified";
        (await stub.SearchAddresses(new()))!.Adresser![0].Adressetekst.ShouldBe("Eksempelveien 1");
        (await stub.SearchAddresses(new() { Sok = "no-match" }))!.Adresser.ShouldBeEmpty();
        (await stub.SearchAddresses(new() { Side = 1 }))!.Adresser.ShouldBeEmpty();
        var filtered = await stub.SearchAddresses(
            new()
            {
                Sok = "eksempelveien",
                Kommunenummer = "0301",
                Postnummer = "0001",
                Gardsnummer = 1,
                Bruksnummer = 1,
            }
        );
        filtered!.Adresser.ShouldHaveSingleItem();
        filtered.Adresser[0].Gardsnummer.ShouldBe(1);
        filtered.Adresser[0].Bruksnummer.ShouldBe(1);
        filtered.Adresser[0].Kommunenummer.ShouldBe("0301");
        filtered.Adresser[0].Postnummer.ShouldBe("0001");
        (await stub.SearchAddresses(new() { Kommunenummer = "0302" }))!.Adresser.ShouldBeEmpty();
        (await stub.SearchAddresses(new() { Postnummer = "0002" }))!.Adresser.ShouldBeEmpty();
        (await stub.SearchAddresses(new() { Gardsnummer = 2 }))!.Adresser.ShouldBeEmpty();
        (await stub.SearchAddresses(new() { Bruksnummer = 2 }))!.Adresser.ShouldBeEmpty();
        (await stub.QuickSearchLocation(new()))!.Epsg.ShouldBe("EPSG:4326");
        (
            await stub.GetClosestAddress(
                new()
                {
                    Lat = 1,
                    Lon = 2,
                    Radius = 100,
                }
            )
        )!.MeterDistanseTilPunkt.ShouldBe(0);
        (await stub.SearchAddressesByPoint(new() { Side = 1 }))!.Adresser.ShouldBeEmpty();
    }

    [Fact]
    public async Task MunicipalityOperationsWorkWithoutNetwork()
    {
        var stub = new FylkeKommuneStub();
        (await stub.GetFylker()).ShouldHaveSingleItem().Fylkesnummer.ShouldBe("03");
        (await stub.GetKommuner()).ShouldHaveSingleItem().Kommunenummer.ShouldBe("0301");
        (await stub.GetFylkerFullInfo()).ShouldHaveSingleItem().Kommuner.ShouldHaveSingleItem();
        (await stub.GetFylkeByNumber("03"))!.Kommuner.ShouldHaveSingleItem();
        (await stub.GetKommuneByNumber("0301"))!.Kommunenavn.ShouldBe("Oslo");
        (await stub.GetKommuneByPoint(new() { Nord = 1, Ost = 2 }))!.Kommunenummer.ShouldBe("0301");
        (await stub.GetFylkeByNumber("99")).ShouldBeNull();
        (await stub.GetKommuneByNumber("9999")).ShouldBeNull();
    }

    [Fact]
    public async Task PropertySearchReturnsPointOrEmptyCollection()
    {
        var stub = new PropertySearchStub();
        var result = await stub.SearchProperties(new() { Matrikkelnummer = "0301-1/1" });
        result!.Type.ShouldBe("FeatureCollection");
        var feature = result.Features.ShouldHaveSingleItem();
        feature.Geometry!.Type.ShouldBe("Point");
        var coordinates = feature
            .Geometry.Coordinates.ShouldBeOfType<UntypedArray>()
            .GetValue()
            .ToArray();
        coordinates[0].ShouldBeOfType<UntypedDouble>().GetValue().ShouldBe(10.7522);
        coordinates[1].ShouldBeOfType<UntypedDouble>().GetValue().ShouldBe(59.9139);
        (await stub.SearchProperties(new() { Gardsnummer = 99 }))!.Features.ShouldBeEmpty();
    }

    [Fact]
    public void UnsupportedFiltersAndCoordinateSystemsFailExplicitly()
    {
        Should.Throw<NotSupportedException>(() =>
            new AddressSearchStub().SearchAddresses(new() { Adressenavn = "Eksempelveien" })
        );
        Should.Throw<NotSupportedException>(() =>
            new AddressSearchStub().SearchAddresses(new() { Utkoordsys = 25833 })
        );
        Should.Throw<NotSupportedException>(() =>
            new AddressSearchStub().SearchAddressesByPoint(new() { Koordsys = 4258 })
        );
        Should.Throw<NotSupportedException>(() =>
            new FylkeKommuneStub().GetKommuneByPoint(new() { Koordsys = 25833 })
        );
        Should.Throw<NotSupportedException>(() =>
            new PropertySearchStub().SearchProperties(new() { Omrade = true })
        );
        Should.Throw<NotSupportedException>(() =>
            new PropertySearchStub().SearchProperties(new() { Utkoordsys = 25833 })
        );
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new AddressSearchStub().SearchAddresses(new() { Side = -1 })
        );
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new AddressSearchStub().SearchAddressesByPoint(new() { TreffPerSide = 0 })
        );
    }
}
