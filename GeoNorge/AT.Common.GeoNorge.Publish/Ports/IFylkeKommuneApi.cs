using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Model.Response;

namespace Arbeidstilsynet.Common.GeoNorge.Ports;

public interface IFylkeKommuneApi
{
    Task<IEnumerable<Fylke>> GetFylker();
    Task<IEnumerable<Kommune>> GetKommuner();
    Task<IEnumerable<FylkeFullInfo>> GetFylkerFullInfo();
    Task<Fylke?> GetFylkeByNumber(string fylkesnummer);
    Task<KommuneFullInfo?> GetKommuneByNumber(string kommunenummer);

    Task<Kommune?> GetKommuneByPoint(PointQuery query);
}