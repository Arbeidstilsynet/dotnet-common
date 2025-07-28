using Arbeidstilsynet.Common.MeldingerReceiver.Model;
using StackExchange.Redis;

namespace Arbeidstilsynet.Common.MeldingerReceiver;

/// <summary>
/// Interface which can be dependency injected to use methods of MeldingerReceiver
/// </summary>
public interface IMeldingerReceiver
{
    /// <summary>
    /// Gets all notifications which have not been read yet
    /// </summary>
    /// <returns></returns>
    Task<Dictionary<string, MeldingerReceiverNotificationDto>> GetNotifications(
        string appId,
        Predicate<MeldingerReceiverNotificationDto>? messageFilter = null
    );

    /// <summary>
    /// Gets all notifications which have not been acknowledged yet
    /// </summary>
    /// <returns></returns>
    Task<StreamEntry[]> GetPendingMessages(string appId);

    /// <summary>
    /// Acknowledges a pending message to remove it from the pending messages list.
    /// The message is not going to get consumed anymore by the same group.
    /// When the message is acknowledged by all groups, it can be deleted.
    /// </summary>
    /// <returns></returns>
    Task<long> AcknowledgeMessage(string groupName, string appId);
}
