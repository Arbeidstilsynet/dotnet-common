using Arbeidstilsynet.Common.FeatureFlags.Model;

namespace Arbeidstilsynet.Common.FeatureFlags.Extensions;

/// <summary>
/// Extensions for FeatureFlags
/// </summary>
public static class FeatureFlagsExtensions
{
    /// <summary>
    /// Dummy extension method for demo
    /// </summary>
    /// <param name="dto">The dto to extend</param>
    /// <returns>Returns the dto with a modified Foo property</returns>
    public static FeatureFlagsDto ToUpper(this FeatureFlagsDto dto)
    {
        return new FeatureFlagsDto { Foo = dto.Foo.ToUpper() };
    }
}
