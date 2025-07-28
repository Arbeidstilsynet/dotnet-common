using Arbeidstilsynet.Common.MeldingerReceiver.Model;

namespace Arbeidstilsynet.Common.MeldingerReceiver;

/// <summary>
/// Interface which can be dependency injected to use methods of MeldingerReceiver
/// </summary>
public interface IMeldingerConsumer
{
    public string AppId { get; }

    public int? PollInterval { get; }

    public Task ConsumeNewNotifications(
        Dictionary<string, MeldingerReceiverNotificationDto> newNotifications
    );
}
