using Arbeidstilsynet.Common.FooBarKlient.Ports;
using Arbeidstilsynet.Common.FooBarKlient.Ports.Model;

namespace Arbeidstilsynet.Common.FooBarKlient.Adapters;

internal class FooBarKlientImplementation : IFooBarKlient
{
    public Task<FooBarKlientDto> Get()
    {
        return Task.FromResult(new FooBarKlientDto { Foo = "Bar Bar" });
    }
}
