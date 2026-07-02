using Arbeidstilsynet.Common.GeoNorge.Adresser.Models;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using PunktsokQueryParameters = Arbeidstilsynet.Common.GeoNorge.Adresser.Punktsok.PunktsokRequestBuilder.PunktsokRequestBuilderGetQueryParameters;
using SokQueryParameters = Arbeidstilsynet.Common.GeoNorge.Adresser.Sok.SokRequestBuilder.SokRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.GeoNorge.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IAddressSearch"/> interface to simplify address and location searches.
/// </summary>
public static class AddressSearchExtensions
{
    /// <summary>
    /// Gets the closest address based on a geographical point defined by the generated query parameters.
    /// </summary>
    /// <param name="addressSearch">The address search service instance.</param>
    /// <param name="queryParameters">The generated query parameters containing coordinates and search radius.</param>
    /// <returns>The closest <see cref="OutputGeoPoint"/> if found, otherwise null.</returns>
    public static async Task<OutputGeoPoint?> GetClosestAddress(
        this IAddressSearch addressSearch,
        PunktsokQueryParameters queryParameters
    )
    {
        queryParameters.Side = 0;
        queryParameters.TreffPerSide = 1;

        var result = await addressSearch.SearchAddressesByPoint(queryParameters);

        return result?.Adresser?.FirstOrDefault();
    }

    /// <summary>
    /// Searches for a location based on the generated query parameters.
    /// </summary>
    /// <param name="addressSearch">The address search service instance.</param>
    /// <param name="queryParameters">The generated query parameters containing the search term and filters.</param>
    /// <returns>The <see cref="GeomPoint"/> (representasjonspunkt) of the first matching address if found, otherwise null.</returns>
    public static async Task<GeomPoint?> QuickSearchLocation(
        this IAddressSearch addressSearch,
        SokQueryParameters queryParameters
    )
    {
        queryParameters.Side = 0;
        queryParameters.TreffPerSide = 1;

        var result = await addressSearch.SearchAddresses(queryParameters);

        return result?.Adresser?.FirstOrDefault()?.Representasjonspunkt;
    }
}
