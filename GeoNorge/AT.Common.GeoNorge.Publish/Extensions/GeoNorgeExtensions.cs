using Arbeidstilsynet.Common.GeoNorge.Model;

namespace Arbeidstilsynet.Common.GeoNorge.Extensions.Something;

/// <summary>
/// Extensions for GeoNorge
/// </summary>
public static class GeoNorgeExtensions
{
    /// <summary>
    /// Dummy extension method for demo
    /// </summary>
    /// <param name="dto">The dto to extend</param>
    /// <returns>Returns the dto with a modified Foo property</returns>
    public static GeoNorgeDto ToUpper(this GeoNorgeDto dto)
    {
        return new GeoNorgeDto { Foo = dto.Foo.ToUpper() };
    }
}
