using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Model.Response;
using Arbeidstilsynet.Common.GeoNorge.Ports;

namespace Arbeidstilsynet.Common.GeoNorge.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IAddressSearch"/> interface to simplify address and location searches.
/// </summary>
public static class AddressSearchExtensions
{
    /// <summary>
    /// Gets the closest address based on a geographical point defined by <see cref="PointSearchQuery"/>.
    /// </summary>
    /// <param name="addressSearch">The address search service instance.</param>
    /// <param name="query">The point search query containing coordinates and search radius.</param>
    /// <returns>The closest <see cref="Address"/> if found, otherwise null.</returns>
    public static async Task<Address?> GetClosestAddress(
        this IAddressSearch addressSearch,
        PointSearchQuery query
    )
    {
        var pagination = new Pagination { PageIndex = 0, PageSize = 1 };

        var result = await addressSearch.SearchAddressesByPoint(query, pagination);

        return result?.Elements.FirstOrDefault();
    }

    /// <summary>
    /// Searches for a location based on a text query defined by <see cref="TextSearchQuery"/>.
    /// </summary>
    /// <param name="addressSearch">The address search service instance.</param>
    /// <param name="query">The text search query containing the search term and filters.</param>
    /// <returns>The <see cref="Location"/> of the first matching address if found, otherwise null.</returns>
    public static async Task<Location?> QuickSearchLocation(
        this IAddressSearch addressSearch,
        TextSearchQuery query
    )
    {
        var pagination = new Pagination { PageIndex = 0, PageSize = 1 };

        var result = await addressSearch.SearchAddresses(query, pagination);

        return result?.Elements.FirstOrDefault()?.Location;
    }
}
