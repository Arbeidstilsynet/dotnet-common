using Xunit;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Arbeidstilsynet.Common.MeldingerReceiver.Adapters.Test.fixtures;

public class ValkeyFixture: IAsyncLifetime
{
    private readonly IContainer _valkeyContainer;
    
    const ushort ValkeyPort = 6379;

    public ValkeyFixture()
    {
        _valkeyContainer = new ContainerBuilder().WithImage("valkey/valkey:latest").WithPortBinding(ValkeyPort, true)
            .Build();
    }
    
    public string ValkeyBaseUrl => $"{_valkeyContainer.Hostname}:{_valkeyContainer.GetMappedPublicPort(ValkeyPort)}";

    public Task InitializeAsync()
    {
        return _valkeyContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _valkeyContainer.StopAsync();
    }
}