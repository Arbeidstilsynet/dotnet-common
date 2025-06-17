using Arbeidstilsynet.Common.BlubExtensions.Model;

namespace Arbeidstilsynet.Common.BlubExtensions.Implementation;

internal class BlubExtensionsImplementation : IBlubExtensions
{
    public Task<BlubExtensionsDto> Get()
    {
        return Task.FromResult(new BlubExtensionsDto { Foo = "Bar" });
    }
}
