using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Model.Response;
using Arbeidstilsynet.Common.GeoNorge.Ports;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal class ApproximateSvalbardAndJanMayenFylkeKommuneApi(IFylkeKommuneApi inner)
    : IFylkeKommuneApi
{
    private static Fylke NewSvalbardFylke() =>
        new()
        {
            Fylkesnummer = "21",
            Fylkesnavn = "Svalbard",
        };

    private static Kommune NewSvalbardKommune() =>
        new()
        {
            Kommunenummer = "2100",
            Kommunenavn = "Svalbard",
        };

    private static KommuneFullInfo NewSvalbardKommuneFullInfo() =>
        new()
        {
            Fylkesnummer = "21",
            Kommune = NewSvalbardKommune(),
            Location = new Location
            {
                Latitude = 78.2232,
                Longitude = 15.6469,
                Epsg = "4326",
            },
        };

    private static FylkeFullInfo NewSvalbardFylkeFullInfo() =>
        new()
        {
            Fylke = NewSvalbardFylke(),
            Kommuner = [NewSvalbardKommuneFullInfo()],
        };

    private static Fylke NewJanMayenFylke() =>
        new()
        {
            Fylkesnummer = "22",
            Fylkesnavn = "Jan Mayen",
        };

    private static Kommune NewJanMayenKommune() =>
        new()
        {
            Kommunenummer = "2211",
            Kommunenavn = "Jan Mayen",
        };

    private static KommuneFullInfo NewJanMayenKommuneFullInfo() =>
        new()
        {
            Fylkesnummer = "22",
            Kommune = NewJanMayenKommune(),
            Location = new Location
            {
                Latitude = 70.9821,
                Longitude = -8.5337,
                Epsg = "4326",
            },
        };

    private static FylkeFullInfo NewJanMayenFylkeFullInfo() =>
        new()
        {
            Fylke = NewJanMayenFylke(),
            Kommuner = [NewJanMayenKommuneFullInfo()],
        };

    public async Task<IEnumerable<Fylke>> GetFylker()
    {
        var fylker = (await inner.GetFylker()).ToList();

        AddIfMissing(fylker, NewSvalbardFylke(), f => f.Fylkesnummer);
        AddIfMissing(fylker, NewJanMayenFylke(), f => f.Fylkesnummer);

        return fylker;
    }

    public async Task<IEnumerable<Kommune>> GetKommuner()
    {
        var kommuner = (await inner.GetKommuner()).ToList();

        AddIfMissing(kommuner, NewSvalbardKommune(), k => k.Kommunenummer);
        AddIfMissing(kommuner, NewJanMayenKommune(), k => k.Kommunenummer);

        return kommuner;
    }

    public async Task<IEnumerable<FylkeFullInfo>> GetFylkerFullInfo()
    {
        var fylker = (await inner.GetFylkerFullInfo()).ToList();

        AddIfMissing(fylker, NewSvalbardFylkeFullInfo(), f => f.Fylke.Fylkesnummer);
        AddIfMissing(fylker, NewJanMayenFylkeFullInfo(), f => f.Fylke.Fylkesnummer);

        return fylker;
    }

    public Task<Fylke?> GetFylkeByNumber(string fylkesnummer)
    {
        return fylkesnummer switch
        {
            "21" => Task.FromResult<Fylke?>(NewSvalbardFylke()),
            "22" => Task.FromResult<Fylke?>(NewJanMayenFylke()),
            _ => inner.GetFylkeByNumber(fylkesnummer),
        };
    }

    public Task<KommuneFullInfo?> GetKommuneByNumber(string kommunenummer)
    {
        return kommunenummer switch
        {
            "2100" => Task.FromResult<KommuneFullInfo?>(NewSvalbardKommuneFullInfo()),
            "2211" => Task.FromResult<KommuneFullInfo?>(NewJanMayenKommuneFullInfo()),
            _ => inner.GetKommuneByNumber(kommunenummer),
        };
    }

    public Task<Kommune?> GetKommuneByPoint(PointQuery query)
    {
        if (UsesGeographicCoordinates(query) && SvalbardBoundingBoxes.Contains(query))
        {
            return Task.FromResult<Kommune?>(NewSvalbardKommune());
        }

        if (UsesGeographicCoordinates(query) && JanMayenBoundingBox.Contains(query))
        {
            return Task.FromResult<Kommune?>(NewJanMayenKommune());
        }

        return inner.GetKommuneByPoint(query);
    }

    private static bool UsesGeographicCoordinates(PointQuery query)
    {
        return query.Epsg is 4258 or 4326;
    }

    private static void AddIfMissing<T>(
        List<T> values,
        T value,
        Func<T, string> keySelector
    )
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

