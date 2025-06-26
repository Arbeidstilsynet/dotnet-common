using Arbeidstilsynet.Common.GeoNorge.Model;

namespace Arbeidstilsynet.Common.GeoNorge;

/// <summary>
/// Implementation of 
/// </summary>
public interface IGeoNorge
{
    /// <summary>
    /// /sok
    /// </summary>
    /// <returns></returns>
    Task<GeoNorgeDto> Search();
    
    /// <summary>
    /// /punktsok
    /// </summary>
    /// <param name="geoNorgeDto"></param>
    /// <returns></returns>
    Task<GeoNorgeDto> PointSearch(GeoNorgeDto geoNorgeDto);
}
