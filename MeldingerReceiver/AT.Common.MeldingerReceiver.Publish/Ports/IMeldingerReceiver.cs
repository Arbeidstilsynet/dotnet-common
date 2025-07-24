using Arbeidstilsynet.Common.MeldingerReceiver.Model;

namespace Arbeidstilsynet.Common.MeldingerReceiver;

/// <summary>
/// Interface which can be dependency injected to use methods of MeldingerReceiver
/// </summary>
public interface IMeldingerReceiver
{
    /// <summary>
    /// Required XML summary of the Get method
    /// </summary>
    /// <returns></returns>
    Task<MeldingerReceiverDto> Get();
}
