using Arbeidstilsynet.Common.GeoNorge.KommuneInfo.Models;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using PunktQueryParameters = Arbeidstilsynet.Common.GeoNorge.KommuneInfo.Punkt.PunktRequestBuilder.PunktRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal class ApproximateSvalbardAndJanMayenFylkeKommuneApi(IFylkeKommuneApi inner)
    : IFylkeKommuneApi
{
    private const string SvalbardName = "Svalbard";
    private const string JanMayenName = "Jan Mayen";

    private static FylkerEnkel NewSvalbardFylke() =>
        new() { Fylkesnummer = "21", Fylkesnavn = SvalbardName };

    private static KomEnkelNorskNavn NewSvalbardKommune() =>
        new()
        {
            Kommunenummer = "2100",
            Kommunenavn = SvalbardName,
            KommunenavnNorsk = SvalbardName,
        };

    private static KomFull NewSvalbardKomFull() =>
        new()
        {
            Fylkesnummer = "21",
            Fylkesnavn = SvalbardName,
            Kommunenummer = "2100",
            Kommunenavn = SvalbardName,
            KommunenavnNorsk = SvalbardName,
            PunktIOmrade = NewPoint(longitude: 15.6469, latitude: 78.2232),
        };

    private static FylkerKommunerFull NewSvalbardFylkeFullInfo() =>
        new()
        {
            Fylkesnummer = "21",
            Fylkesnavn = SvalbardName,
            Kommuner = [NewSvalbardKomFull()],
        };

    private static FylkerKommunerEnkel NewSvalbardFylkerKommunerEnkel() =>
        new()
        {
            Fylkesnummer = "21",
            Fylkesnavn = SvalbardName,
            Kommuner = [new KomEnkel { Kommunenummer = "2100", Kommunenavn = SvalbardName }],
        };

    private static KommuneFylkeEnkel NewSvalbardKommuneFylkeEnkel() =>
        new()
        {
            Fylkesnummer = "21",
            Fylkesnavn = SvalbardName,
            Kommunenummer = "2100",
            Kommunenavn = SvalbardName,
        };

    private static FylkerEnkel NewJanMayenFylke() =>
        new() { Fylkesnummer = "22", Fylkesnavn = JanMayenName };

    private static KomEnkelNorskNavn NewJanMayenKommune() =>
        new()
        {
            Kommunenummer = "2211",
            Kommunenavn = JanMayenName,
            KommunenavnNorsk = JanMayenName,
        };

    private static KomFull NewJanMayenKomFull() =>
        new()
        {
            Fylkesnummer = "22",
            Fylkesnavn = JanMayenName,
            Kommunenummer = "2211",
            Kommunenavn = JanMayenName,
            KommunenavnNorsk = JanMayenName,
            PunktIOmrade = NewPoint(longitude: -8.5337, latitude: 70.9821),
        };

    private static FylkerKommunerFull NewJanMayenFylkeFullInfo() =>
        new()
        {
            Fylkesnummer = "22",
            Fylkesnavn = JanMayenName,
            Kommuner = [NewJanMayenKomFull()],
        };

    private static FylkerKommunerEnkel NewJanMayenFylkerKommunerEnkel() =>
        new()
        {
            Fylkesnummer = "22",
            Fylkesnavn = JanMayenName,
            Kommuner = [new KomEnkel { Kommunenummer = "2211", Kommunenavn = JanMayenName }],
        };

    private static KommuneFylkeEnkel NewJanMayenKommuneFylkeEnkel() =>
        new()
        {
            Fylkesnummer = "22",
            Fylkesnavn = JanMayenName,
            Kommunenummer = "2211",
            Kommunenavn = JanMayenName,
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

    public Task<KommuneFylkeEnkel?> GetKommuneByPoint(PunktQueryParameters queryParameters)
    {
        if (
            UsesGeographicCoordinates(queryParameters)
            && SvalbardBoundingBoxes.Contains(queryParameters)
        )
        {
            return Task.FromResult<KommuneFylkeEnkel?>(NewSvalbardKommuneFylkeEnkel());
        }

        if (
            UsesGeographicCoordinates(queryParameters)
            && JanMayenBoundingBox.Contains(queryParameters)
        )
        {
            return Task.FromResult<KommuneFylkeEnkel?>(NewJanMayenKommuneFylkeEnkel());
        }

        return inner.GetKommuneByPoint(queryParameters);
    }

    private static GeoJson NewPoint(double longitude, double latitude) =>
        new() { Type = "Point", Coordinates = [longitude, latitude] };

    private static bool UsesGeographicCoordinates(PunktQueryParameters query)
    {
        return query.Koordsys is 4258 or 4326;
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
        public static bool Contains(PunktQueryParameters query) =>
            SvalbardMainlandBoundingBox.Contains(query)
            || BjornoyaBoundingBox.Contains(query)
            || HopenBoundingBox.Contains(query);
    }

    private static class SvalbardMainlandBoundingBox
    {
        public static bool Contains(PunktQueryParameters query) =>
            query.Nord is >= 74.0 and <= 81.5 && query.Ost is >= 10.0 and <= 35.5;
    }

    private static class BjornoyaBoundingBox
    {
        public static bool Contains(PunktQueryParameters query) =>
            query.Nord is >= 74.2 and <= 74.7 && query.Ost is >= 18.7 and <= 19.5;
    }

    private static class HopenBoundingBox
    {
        public static bool Contains(PunktQueryParameters query) =>
            query.Nord is >= 76.3 and <= 76.8 && query.Ost is >= 24.5 and <= 25.6;
    }

    private static class JanMayenBoundingBox
    {
        public static bool Contains(PunktQueryParameters query) =>
            query.Nord is >= 70.5 and <= 71.5 && query.Ost is >= -10.0 and <= -7.0;
    }
}
