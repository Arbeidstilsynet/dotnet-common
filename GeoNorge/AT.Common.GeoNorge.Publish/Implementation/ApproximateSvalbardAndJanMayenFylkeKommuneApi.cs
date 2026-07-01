using Arbeidstilsynet.Common.GeoNorge.KommuneInfo.Models;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Ports;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal class ApproximateSvalbardAndJanMayenFylkeKommuneApi(IFylkeKommuneApi inner)
    : IFylkeKommuneApi
{
    private static FylkerEnkel NewSvalbardFylke() =>
        new() { Fylkesnummer = "21", Fylkesnavn = "Svalbard" };

    private static KomEnkelNorskNavn NewSvalbardKommune() =>
        new()
        {
            Kommunenummer = "2100",
            Kommunenavn = "Svalbard",
            KommunenavnNorsk = "Svalbard",
        };

    private static KomFull NewSvalbardKomFull() =>
        new()
        {
            Fylkesnummer = "21",
            Fylkesnavn = "Svalbard",
            Kommunenummer = "2100",
            Kommunenavn = "Svalbard",
            KommunenavnNorsk = "Svalbard",
            PunktIOmrade = NewPoint(longitude: 15.6469, latitude: 78.2232),
        };

    private static FylkerKommunerFull NewSvalbardFylkeFullInfo() =>
        new()
        {
            Fylkesnummer = "21",
            Fylkesnavn = "Svalbard",
            Kommuner = [NewSvalbardKomFull()],
        };

    private static FylkerKommunerEnkel NewSvalbardFylkerKommunerEnkel() =>
        new()
        {
            Fylkesnummer = "21",
            Fylkesnavn = "Svalbard",
            Kommuner = [new KomEnkel { Kommunenummer = "2100", Kommunenavn = "Svalbard" }],
        };

    private static KommuneFylkeEnkel NewSvalbardKommuneFylkeEnkel() =>
        new()
        {
            Fylkesnummer = "21",
            Fylkesnavn = "Svalbard",
            Kommunenummer = "2100",
            Kommunenavn = "Svalbard",
        };

    private static FylkerEnkel NewJanMayenFylke() =>
        new() { Fylkesnummer = "22", Fylkesnavn = "Jan Mayen" };

    private static KomEnkelNorskNavn NewJanMayenKommune() =>
        new()
        {
            Kommunenummer = "2211",
            Kommunenavn = "Jan Mayen",
            KommunenavnNorsk = "Jan Mayen",
        };

    private static KomFull NewJanMayenKomFull() =>
        new()
        {
            Fylkesnummer = "22",
            Fylkesnavn = "Jan Mayen",
            Kommunenummer = "2211",
            Kommunenavn = "Jan Mayen",
            KommunenavnNorsk = "Jan Mayen",
            PunktIOmrade = NewPoint(longitude: -8.5337, latitude: 70.9821),
        };

    private static FylkerKommunerFull NewJanMayenFylkeFullInfo() =>
        new()
        {
            Fylkesnummer = "22",
            Fylkesnavn = "Jan Mayen",
            Kommuner = [NewJanMayenKomFull()],
        };

    private static FylkerKommunerEnkel NewJanMayenFylkerKommunerEnkel() =>
        new()
        {
            Fylkesnummer = "22",
            Fylkesnavn = "Jan Mayen",
            Kommuner = [new KomEnkel { Kommunenummer = "2211", Kommunenavn = "Jan Mayen" }],
        };

    private static KommuneFylkeEnkel NewJanMayenKommuneFylkeEnkel() =>
        new()
        {
            Fylkesnummer = "22",
            Fylkesnavn = "Jan Mayen",
            Kommunenummer = "2211",
            Kommunenavn = "Jan Mayen",
        };

    public async Task<IEnumerable<FylkerEnkel>> GetFylker()
    {
        var fylker = (await inner.GetFylker()).ToList();

        AddIfMissing(fylker, NewSvalbardFylke(), f => f.Fylkesnummer);
        AddIfMissing(fylker, NewJanMayenFylke(), f => f.Fylkesnummer);

        return fylker;
    }

    public async Task<IEnumerable<KomEnkelNorskNavn>> GetKommuner()
    {
        var kommuner = (await inner.GetKommuner()).ToList();

        AddIfMissing(kommuner, NewSvalbardKommune(), k => k.Kommunenummer);
        AddIfMissing(kommuner, NewJanMayenKommune(), k => k.Kommunenummer);

        return kommuner;
    }

    public async Task<IEnumerable<FylkerKommunerFull>> GetFylkerFullInfo()
    {
        var fylker = (await inner.GetFylkerFullInfo()).ToList();

        AddIfMissing(fylker, NewSvalbardFylkeFullInfo(), f => f.Fylkesnummer);
        AddIfMissing(fylker, NewJanMayenFylkeFullInfo(), f => f.Fylkesnummer);

        return fylker;
    }

    public Task<FylkerKommunerEnkel?> GetFylkeByNumber(string fylkesnummer)
    {
        return fylkesnummer switch
        {
            "21" => Task.FromResult<FylkerKommunerEnkel?>(NewSvalbardFylkerKommunerEnkel()),
            "22" => Task.FromResult<FylkerKommunerEnkel?>(NewJanMayenFylkerKommunerEnkel()),
            _ => inner.GetFylkeByNumber(fylkesnummer),
        };
    }

    public Task<KomFull?> GetKommuneByNumber(string kommunenummer)
    {
        return kommunenummer switch
        {
            "2100" => Task.FromResult<KomFull?>(NewSvalbardKomFull()),
            "2211" => Task.FromResult<KomFull?>(NewJanMayenKomFull()),
            _ => inner.GetKommuneByNumber(kommunenummer),
        };
    }

    public Task<KommuneFylkeEnkel?> GetKommuneByPoint(PointQuery query)
    {
        if (UsesGeographicCoordinates(query) && SvalbardBoundingBoxes.Contains(query))
        {
            return Task.FromResult<KommuneFylkeEnkel?>(NewSvalbardKommuneFylkeEnkel());
        }

        if (UsesGeographicCoordinates(query) && JanMayenBoundingBox.Contains(query))
        {
            return Task.FromResult<KommuneFylkeEnkel?>(NewJanMayenKommuneFylkeEnkel());
        }

        return inner.GetKommuneByPoint(query);
    }

    private static GeoJson NewPoint(double longitude, double latitude) =>
        new()
        {
            Type = "Point",
            Coordinates = [longitude, latitude],
        };

    private static bool UsesGeographicCoordinates(PointQuery query)
    {
        return query.Epsg is 4258 or 4326;
    }

    private static void AddIfMissing<T>(List<T> values, T value, Func<T, string?> keySelector)
    {
        var key = keySelector(value);
        if (values.All(v => keySelector(v) != key))
        {
            values.Add(value);
        }
    }

    private static class SvalbardBoundingBoxes
    {
        public static bool Contains(PointQuery query) =>
            SvalbardMainlandBoundingBox.Contains(query)
            || BjornoyaBoundingBox.Contains(query)
            || HopenBoundingBox.Contains(query);
    }

    private static class SvalbardMainlandBoundingBox
    {
        public static bool Contains(PointQuery query) =>
            query.Latitude is >= 74.0 and <= 81.5 && query.Longitude is >= 10.0 and <= 35.5;
    }

    private static class BjornoyaBoundingBox
    {
        public static bool Contains(PointQuery query) =>
            query.Latitude is >= 74.2 and <= 74.7 && query.Longitude is >= 18.7 and <= 19.5;
    }

    private static class HopenBoundingBox
    {
        public static bool Contains(PointQuery query) =>
            query.Latitude is >= 76.3 and <= 76.8 && query.Longitude is >= 24.5 and <= 25.6;
    }

    private static class JanMayenBoundingBox
    {
        public static bool Contains(PointQuery query) =>
            query.Latitude is >= 70.5 and <= 71.5 && query.Longitude is >= -10.0 and <= -7.0;
    }
}
