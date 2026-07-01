using Arbeidstilsynet.Common.GeoNorge.KommuneInfo.Models;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;

namespace Arbeidstilsynet.Common.GeoNorge.Ports;

/// <summary>
/// Interface for accessing Norwegian county (fylke) and municipality (kommune) information through the GeoNorge API.
/// </summary>
public interface IFylkeKommuneApi
{
    /// <summary>
    /// Retrieves all Norwegian counties (fylker).
    /// </summary>
    /// <returns>A collection of generated <see cref="FylkerEnkel"/> objects representing all counties.</returns>
    Task<IEnumerable<FylkerEnkel>> GetFylker();

    /// <summary>
    /// Retrieves all Norwegian municipalities (kommuner).
    /// </summary>
    /// <returns>A collection of generated <see cref="KomEnkelNorskNavn"/> objects representing all municipalities.</returns>
    Task<IEnumerable<KomEnkelNorskNavn>> GetKommuner();

    /// <summary>
    /// Retrieves detailed information for all Norwegian counties (fylker).
    /// </summary>
    /// <returns>A collection of generated <see cref="FylkerKommunerFull"/> objects with full county details.</returns>
    Task<IEnumerable<FylkerKommunerFull>> GetFylkerFullInfo();

    /// <summary>
    /// Retrieves a specific county (fylke) by its number.
    /// </summary>
    /// <param name="fylkesnummer">The county number (e.g., "03" for Oslo).</param>
    /// <returns>A generated <see cref="FylkerKommunerEnkel"/> object if found, otherwise null.</returns>
    Task<FylkerKommunerEnkel?> GetFylkeByNumber(string fylkesnummer);

    /// <summary>
    /// Retrieves detailed information for a specific municipality (kommune) by its number.
    /// </summary>
    /// <param name="kommunenummer">The municipality number (e.g., "0301" for Oslo).</param>
    /// <returns>A generated <see cref="KomFull"/> object if found, otherwise null.</returns>
    Task<KomFull?> GetKommuneByNumber(string kommunenummer);

    /// <summary>
    /// Finds the municipality (kommune) that contains the specified geographical point.
    /// </summary>
    /// <param name="query">The geographical point query with coordinates.</param>
    /// <returns>A generated <see cref="KommuneFylkeEnkel"/> object if found, otherwise null.</returns>
    Task<KommuneFylkeEnkel?> GetKommuneByPoint(PointQuery query);
}
