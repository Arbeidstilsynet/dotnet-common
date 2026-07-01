using System.Text.RegularExpressions;
using Arbeidstilsynet.Common.GeoNorge.KommuneInfo;
using Arbeidstilsynet.Common.GeoNorge.KommuneInfo.Models;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Ports;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal partial class FylkeKommuneClient(KommuneInfoClient client) : IFylkeKommuneApi
{
    public async Task<IEnumerable<FylkerEnkel>> GetFylker()
    {
        return await client.Fylker.GetAsync() ?? [];
    }

    public async Task<IEnumerable<KomEnkelNorskNavn>> GetKommuner()
    {
        return await client.Kommuner.GetAsync() ?? [];
    }

    public async Task<IEnumerable<FylkerKommunerFull>> GetFylkerFullInfo()
    {
        return await client.Fylkerkommuner.GetAsync() ?? [];
    }

    public async Task<FylkerKommunerEnkel?> GetFylkeByNumber(string fylkesnummer)
    {
        if (!FylkesnummerRegex().IsMatch(fylkesnummer))
        {
            throw new ArgumentException(
                $"Invalid fylkesnummer format: {fylkesnummer}. Must be 2 digits.",
                nameof(fylkesnummer)
            );
        }

        return await client.Fylker[fylkesnummer].GetAsync();
    }

    public async Task<KomFull?> GetKommuneByNumber(string kommunenummer)
    {
        if (!KommunenummerRegex().IsMatch(kommunenummer))
        {
            throw new ArgumentException(
                $"Invalid kommunenummer format: {kommunenummer}. Must be 4 digits.",
                nameof(kommunenummer)
            );
        }

        return await client.Kommuner[kommunenummer].GetAsync();
    }

    public async Task<KommuneFylkeEnkel?> GetKommuneByPoint(PointQuery query)
    {
        return await client.Punkt.GetAsync(config =>
        {
            config.QueryParameters.Nord = query.Latitude;
            config.QueryParameters.Ost = query.Longitude;
            config.QueryParameters.Koordsys = query.Epsg;
        });
    }

    [GeneratedRegex(@"^\d{4}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
    private static partial Regex KommunenummerRegex();

    [GeneratedRegex(@"^\d{2}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
    private static partial Regex FylkesnummerRegex();
}
