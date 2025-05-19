using Arbeidstilsynet.Common.FooBarKlient.Ports.Model;

namespace Arbeidstilsynet.Common.FooBarKlient.Ports;

/// <summary>
/// Interface which can be dependency injected to use mehtods of FooBarKlient
/// </summary>
public interface IFooBarKlient
{
    Task<FooBarKlientDto> Get();
}
