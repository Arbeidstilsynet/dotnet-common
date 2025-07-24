using Arbeidstilsynet.Common.MeldingerReceiver.Model;

namespace Arbeidstilsynet.Common.MeldingerReceiver.Extensions.Something;

/// <summary>
/// Extensions for MeldingerReceiver
/// </summary>
public static class MeldingerReceiverExtensions
{
    /// <summary>
    /// Dummy extension method for demo
    /// </summary>
    /// <param name="dto">The dto to extend</param>
    /// <returns>Returns the dto with a modified Foo property</returns>
    public static MeldingerReceiverDto ToUpper(this MeldingerReceiverDto dto)
    {
        return new MeldingerReceiverDto { Foo = dto.Foo.ToUpper() };
    }
}
