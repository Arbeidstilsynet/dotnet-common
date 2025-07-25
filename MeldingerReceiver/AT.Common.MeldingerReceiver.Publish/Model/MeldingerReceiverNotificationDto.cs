namespace Arbeidstilsynet.Common.MeldingerReceiver.Model;

/// <summary>
/// Dto for the meldinger receiver event bus
/// </summary>
public record MeldingerReceiverNotificationDto
{
    /// <summary>
    /// AppId which can be used for filtering received notifications
    /// </summary>
    public required string AppId { get; init; }

    /// <summary>
    /// Timestamp when this notification was created
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
