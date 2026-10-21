using Arbeidstilsynet.Common.GeoNorge.Adresser.Models;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Arbeidstilsynet.Shared.Implementation;
using PunktsokQueryParameters = Arbeidstilsynet.Common.GeoNorge.Adresser.Punktsok.PunktsokRequestBuilder.PunktsokRequestBuilderGetQueryParameters;
using SokQueryParameters = Arbeidstilsynet.Common.GeoNorge.Adresser.Sok.SokRequestBuilder.SokRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal sealed class AddressSearchStub : IAddressSearch
{
    private const string Address = "Eksempelveien 1";

    public Task<OutputAdresseList?> SearchAddresses(SokQueryParameters queryParameters)
    {
        StubQuery.Validate(
            queryParameters,
            "Sok",
            "Adressetekst",
            "Kommunenummer",
            "Postnummer",
            "Gardsnummer",
            "Bruksnummer",
            "Side",
            "TreffPerSide",
            "Utkoordsys"
        );
        StubQuery.GeographicCoordinates(queryParameters.Utkoordsys);
        StubQuery.Page(queryParameters.Side, queryParameters.TreffPerSide);
        var matches =
            Matches(queryParameters.Sok)
            && Matches(queryParameters.Adressetekst)
            && Matches(queryParameters.Kommunenummer, "0301")
            && Matches(queryParameters.Postnummer, "0001")
            && Matches(queryParameters.Gardsnummer, 1)
            && Matches(queryParameters.Bruksnummer, 1);
        var hasResult = matches && (queryParameters.Side ?? 0) == 0;
        return Task.FromResult<OutputAdresseList?>(
            new()
            {
                Adresser = hasResult
                    ?
                    [
                        new OutputAdresse
                        {
                            Adressenavn = "Eksempelveien",
                            Adressetekst = Address,
                            Nummer = 1,
                            Kommunenummer = "0301",
                            Kommunenavn = "Oslo",
                            Postnummer = "0001",
                            Poststed = "Oslo",
                            Gardsnummer = 1,
                            Bruksnummer = 1,
                            Representasjonspunkt = NewPoint(),
                        },
                    ]
                    : [],
                Metadata = Metadata(
                    queryParameters.Side,
                    queryParameters.TreffPerSide,
                    matches,
                    hasResult
                ),
            }
        );
    }

    public Task<OutputGeoPointList?> SearchAddressesByPoint(PunktsokQueryParameters queryParameters)
    {
        StubQuery.Validate(
            queryParameters,
            "Lat",
            "Lon",
            "Radius",
            "Side",
            "TreffPerSide",
            "Koordsys",
            "Utkoordsys"
        );
        StubQuery.GeographicCoordinates(queryParameters.Koordsys);
        StubQuery.GeographicCoordinates(queryParameters.Utkoordsys);
        StubQuery.Page(queryParameters.Side, queryParameters.TreffPerSide);
        var hasResult = (queryParameters.Side ?? 0) == 0;
        return Task.FromResult<OutputGeoPointList?>(
            new()
            {
                Adresser = hasResult
                    ?
                    [
                        new OutputGeoPoint
                        {
                            Adressenavn = "Eksempelveien",
                            Adressetekst = Address,
                            Nummer = 1,
                            Kommunenummer = "0301",
                            Kommunenavn = "Oslo",
                            Representasjonspunkt = NewPoint(),
                            MeterDistanseTilPunkt = 0,
                        },
                    ]
                    : [],
                Metadata = Metadata(
                    queryParameters.Side,
                    queryParameters.TreffPerSide,
                    true,
                    hasResult
                ),
            }
        );
    }

    private static bool Matches(string? search) =>
        string.IsNullOrEmpty(search)
        || Address.Contains(search, StringComparison.OrdinalIgnoreCase);

    private static bool Matches(string? filter, string value) => filter is null || filter == value;

    private static bool Matches(int? filter, int value) => filter is null || filter == value;

    private static GeomPoint NewPoint() =>
        new()
        {
            Epsg = "EPSG:4326",
            Lat = 59.9139f,
            Lon = 10.7522f,
        };

    private static HitMetadata Metadata(int? page, int? size, bool matches, bool hasResult) =>
        new()
        {
            Side = page ?? 0,
            TreffPerSide = size ?? 10,
            TotaltAntallTreff = matches ? 1 : 0,
            ViserFra = hasResult ? 0 : null,
            ViserTil = hasResult ? 0 : null,
        };
}
