using Arbeidstilsynet.Common.FeatureFlags.DependencyInjection;
using Arbeidstilsynet.Common.FeatureFlags.Model;

namespace Arbeidstilsynet.Common.FeatureFlags.Ports;

/// <summary>
/// Use the <see cref="DependencyInjectionExtensions.AddFeatureFlags"/> method to inject an implementation of this interface.
/// </summary>
public interface IFeatureFlags
{
    /// <summary>
    /// Required XML summary of the Get method
    /// </summary>
    /// <returns></returns>
    public Task<FeatureFlagsDto> Get();
}
