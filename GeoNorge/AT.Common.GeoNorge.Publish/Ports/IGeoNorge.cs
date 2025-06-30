using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Model.Response;

namespace Arbeidstilsynet.Common.GeoNorge.Ports;

/// <summary>
/// Implements the GeoNorge API for address and location searches.
/// </summary>
public interface IGeoNorge
{
    /// <summary>
    /// Implements the "/sok" endpoint for searching for addresses based on the <see cref="TextSearchQuery"/> query. 
    /// </summary>
    /// <param name="query"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<PaginationResult<Address>?> SearchAddresses(TextSearchQuery query, Pagination? pagination=default);

    /// <summary>
    /// Implements the "/punktsok" endpoint for finding the closest address based on a geographical point and a radius defined by <see cref="PointSearchQuery"/>.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<PaginationResult<Address>?> SearchAddressesByPoint(PointSearchQuery query, Pagination? pagination=default);
}
