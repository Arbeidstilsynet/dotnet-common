using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Model.Response;

namespace Arbeidstilsynet.Common.GeoNorge.Ports;

/// <summary>
/// Interface for accessing Norwegian county (fylke) and municipality (kommune) information through the GeoNorge API.
/// </summary>
public interface IFylkeKommuneApi
{
    /// <summary>
    /// Retrieves all Norwegian counties (fylker).
    /// </summary>
    /// <returns>A collection of <see cref="Fylke"/> objects representing all counties.</returns>
    Task<IEnumerable<Fylke>> GetFylker();

    /// <summary>
    /// Retrieves all Norwegian municipalities (kommuner).
    /// </summary>
    /// <returns>A collection of <see cref="Kommune"/> objects representing all municipalities.</returns>
    Task<IEnumerable<Kommune>> GetKommuner();

    /// <summary>
    /// Retrieves detailed information for all Norwegian counties (fylker).
    /// </summary>
    /// <returns>A collection of <see cref="FylkeFullInfo"/> objects with full county details.</returns>
    Task<IEnumerable<FylkeFullInfo>> GetFylkerFullInfo();

    /// <summary>
    /// Retrieves a specific county (fylke) by its number.
    /// </summary>
    /// <param name="fylkesnummer">The county number (e.g., "03" for Oslo).</param>
    /// <returns>A <see cref="Fylke"/> object if found, otherwise null.</returns>
    Task<Fylke?> GetFylkeByNumber(string fylkesnummer);

    /// <summary>
    /// Retrieves detailed information for a specific municipality (kommune) by its number.
    /// </summary>
    /// <param name="kommunenummer">The municipality number (e.g., "0301" for Oslo).</param>
    /// <returns>A <see cref="KommuneFullInfo"/> object if found, otherwise null.</returns>
    Task<KommuneFullInfo?> GetKommuneByNumber(string kommunenummer);

    /// <summary>
    /// Finds the municipality (kommune) that contains the specified geographical point.
    /// </summary>
    /// <param name="query">The geographical point query with coordinates.</param>
    /// <returns>A <see cref="Kommune"/> object if found, otherwise null.</returns>
    Task<Kommune?> GetKommuneByPoint(PointQuery query);
}