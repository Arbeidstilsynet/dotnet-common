using Arbeidstilsynet.Common.BlubExtensions.Model;

namespace Arbeidstilsynet.Common.BlubExtensions.Extensions.Something;

/// <summary>
/// Extensions for BlubExtensions
/// </summary>
public static class BlubExtensionsExtensions
{
    /// <summary>
    /// Dummy extension method for demo
    /// </summary>
    /// <param name="dto">The dto to extend</param>
    /// <returns>Returns the dto with a modified Foo property</returns>
    public static BlubExtensionsDto ToUpper(this BlubExtensionsDto dto)
    {
        return new BlubExtensionsDto { Foo = dto.Foo.ToUpper() };
    }
}
