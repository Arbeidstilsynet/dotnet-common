using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Model.Response;

namespace Arbeidstilsynet.Common.GeoNorge.Ports;

/// <summary>
/// Implements the GeoNorge API for address and location searches.
/// </summary>
public interface IAddressSearch
{
    /// <summary>
    /// Implements the "/sok" endpoint for searching for addresses based on the <see cref="TextSearchQuery"/> query.
    /// </summary>
    /// <param name="query">The text search query containing search terms and filters.</param>
    /// <param name="pagination">Optional pagination parameters. Uses default pagination if not specified.</param>
    /// <returns>A paginated result containing matching addresses, or null if the search failed.</returns>
    Task<PaginationResult<Address>?> SearchAddresses(
        TextSearchQuery query,
        Pagination? pagination = default
    );

    /// <summary>
    /// Implements the "/punktsok" endpoint for finding the closest address based on a geographical point and a radius defined by <see cref="PointSearchQuery"/>.
    /// </summary>
    /// <param name="query">The point search query containing coordinates and search radius.</param>
    /// <param name="pagination">Optional pagination parameters. Uses default pagination if not specified.</param>
    /// <returns>A paginated result containing addresses within the specified radius, or null if the search failed.</returns>
    Task<PaginationResult<Address>?> SearchAddressesByPoint(
        PointSearchQuery query,
        Pagination? pagination = default
    );
}
