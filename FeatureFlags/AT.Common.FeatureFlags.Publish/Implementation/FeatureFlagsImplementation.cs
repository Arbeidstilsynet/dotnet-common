using Arbeidstilsynet.Common.FeatureFlags.Model;
using Arbeidstilsynet.Common.FeatureFlags.Ports;

namespace Arbeidstilsynet.Common.FeatureFlags.Implementation;

/// <summary>
/// Implementations should not be public.
/// </summary>
internal class FeatureFlagsImplementation : IFeatureFlags
{
    public Task<FeatureFlagsDto> Get()
    {
        return Task.FromResult(new FeatureFlagsDto { Foo = "Bar" });
    }
}
