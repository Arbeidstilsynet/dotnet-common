using Arbeidstilsynet.Common.GeoNorge.Eiendom.Models;
using GeokodingQueryParameters = Arbeidstilsynet.Common.GeoNorge.Eiendom.Geokoding.GeokodingRequestBuilder.GeokodingRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.GeoNorge.Ports;

/// <summary>
/// Searches Kartverket cadastral property locations independently of registered addresses.
/// </summary>
public interface IPropertySearch
{
    /// <summary>
    /// Implements "/geokoding" for a cadastral number or its municipality, farm and usage components.
    /// </summary>
    /// <param name="queryParameters">The generated query parameters identifying the property.</param>
    /// <remarks>
    /// Output coordinates default to EPSG:4326 (WGS84) when <c>utkoordsys</c> is omitted.
    /// Point coordinates are ordered longitude, latitude in that coordinate system.
    /// All matching parcels are returned without selecting a main parcel or an incident location.
    /// Setting <c>omrade</c> to true requests area geometries instead of representative points.
    /// Coordinates retain the generated, untyped GeoJSON representation.
    /// </remarks>
    /// <returns>
    /// The generated GeoJSON feature collection, including an empty collection for a successful
    /// search without matches, or null if the request failed. Network and API failures are logged;
    /// unexpected exceptions propagate.
    /// </returns>
    Task<GeoKodingRespons?> SearchProperties(GeokodingQueryParameters queryParameters);
}
