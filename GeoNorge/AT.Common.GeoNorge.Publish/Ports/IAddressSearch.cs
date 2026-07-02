using Arbeidstilsynet.Common.GeoNorge.Adresser.Models;
using PunktsokQueryParameters = Arbeidstilsynet.Common.GeoNorge.Adresser.Punktsok.PunktsokRequestBuilder.PunktsokRequestBuilderGetQueryParameters;
using SokQueryParameters = Arbeidstilsynet.Common.GeoNorge.Adresser.Sok.SokRequestBuilder.SokRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.GeoNorge.Ports;

/// <summary>
/// Implements the GeoNorge API for address and location searches.
/// </summary>
public interface IAddressSearch
{
    /// <summary>
    /// Implements the "/sok" endpoint for searching for addresses based on the generated query parameters.
    /// </summary>
    /// <param name="queryParameters">The generated query parameters containing search terms and filters.</param>
    /// <remarks>
    /// If the output coordinate system (<c>utkoordsys</c>) is not specified on <paramref name="queryParameters"/>,
    /// it defaults to EPSG:4326 (WGS84) instead of the GeoNorge API default of EPSG:4258.
    /// </remarks>
    /// <returns>The generated <see cref="OutputAdresseList"/> containing matching addresses and pagination metadata, or null if the search failed.</returns>
    Task<OutputAdresseList?> SearchAddresses(SokQueryParameters queryParameters);

    /// <summary>
    /// Implements the "/punktsok" endpoint for finding addresses based on a geographical point and a radius.
    /// </summary>
    /// <param name="queryParameters">The generated query parameters containing coordinates and search radius.</param>
    /// <remarks>
    /// If the input coordinate system (<c>koordsys</c>) and/or the output coordinate system (<c>utkoordsys</c>)
    /// are not specified on <paramref name="queryParameters"/>, they default to EPSG:4326 (WGS84) instead of the
    /// GeoNorge API default of EPSG:4258.
    /// </remarks>
    /// <returns>The generated <see cref="OutputGeoPointList"/> containing addresses within the specified radius and pagination metadata, or null if the search failed.</returns>
    Task<OutputGeoPointList?> SearchAddressesByPoint(PunktsokQueryParameters queryParameters);
}
