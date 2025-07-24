using Arbeidstilsynet.Common.MeldingerReceiver.Model;

namespace Arbeidstilsynet.Common.MeldingerReceiver.Implementation;

internal class MeldingerReceiverImplementation : IMeldingerReceiver
{
    public Task<MeldingerReceiverDto> Get()
    {
        return Task.FromResult(new MeldingerReceiverDto { Foo = "Bar" });
    }
}
