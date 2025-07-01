using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Arbeidstilsynet.Common.GeoNorge.Model.Response;
using Arbeidstilsynet.Common.GeoNorge.Ports;

namespace Arbeidstilsynet.Common.GeoNorge.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IGeoNorge"/> interface to simplify address and location searches.
/// </summary>
public static class GeoNorgeExtensions
{
    /// <summary>
    /// Gets the closest address based on a geographical point defined by <see cref="PointSearchQuery"/>.
    /// </summary>
    /// <param name="geoNorge"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public static async Task<Address?> GetClosestAddress(
        this IGeoNorge geoNorge,
        PointSearchQuery query
    )
    {
        var pagination = new Pagination { PageIndex = 0, PageSize = 1 };

        var result = await geoNorge.SearchAddressesByPoint(query, pagination);

        return result?.Elements.FirstOrDefault();
    }

    /// <summary>
    /// Searches for a location based on a text query defined by <see cref="TextSearchQuery"/>.
    /// </summary>
    /// <param name="geoNorge"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public static async Task<Location?> QuickSearchLocation(
        this IGeoNorge geoNorge,
        TextSearchQuery query
    )
    {
        var pagination = new Pagination { PageIndex = 0, PageSize = 1 };

        var result = await geoNorge.SearchAddresses(query, pagination);

        return result?.Elements.FirstOrDefault()?.Location;
    }
}
