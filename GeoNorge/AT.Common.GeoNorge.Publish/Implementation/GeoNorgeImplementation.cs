using Arbeidstilsynet.Common.GeoNorge.Model;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal class GeoNorgeImplementation : IGeoNorge
{
    public Task<GeoNorgeDto> Get()
    {
        return Task.FromResult(new GeoNorgeDto { Foo = "Bar" });
    }
}
