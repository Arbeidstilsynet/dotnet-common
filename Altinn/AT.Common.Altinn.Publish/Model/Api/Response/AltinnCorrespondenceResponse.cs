using System.Text.Json.Serialization;

/*
** Plain copy of altinn dtos with some reductions.
** Since altinn is not providing a dedicated nuget package only for dtos, we added this to avoid that consumers need to include the complete altinn core package.
*/
namespace Arbeidstilsynet.Common.Altinn.Model.Api.Response;

/// <summary>
/// Contains information about the created correspondences and their attachments.
/// </summary>
public class CorrespondenceResponse
{
    /// <summary>
    /// The initialized correspondences
    /// </summary>
    [JsonPropertyName("correspondences")]
    public List<InitializedCorrespondences>? Correspondences { get; set; }

    /// <summary>
    /// The IDs of the attachments that is included in the correspondences
    /// </summary>
    [JsonPropertyName("attachmentIds")]
    public List<Guid>? AttachmentIds { get; set; }
}

/// <summary>
/// Represents a correspondence that has been initialized
/// </summary>
public class InitializedCorrespondences
{
    /// <summary>
    /// The ID of the correspondence
    /// </summary>
    [JsonPropertyName("correspondenceId")]
    public Guid CorrespondenceId { get; set; }

    /// <summary>
    /// The current status of the correspondence
    /// </summary>
    [JsonPropertyName("status")]
    public CorrespondenceStatus Status { get; set; }

    /// <summary>
    /// The recipient of the correspondence
    /// </summary>
    [JsonPropertyName("recipient")]
    public required string Recipient { get; set; }

    /// <summary>
    /// Information about the notifications that were created for the correspondence
    /// </summary>
    [JsonPropertyName("notifications")]
    public List<InitializedCorrespondencesNotifications>? Notifications { get; set; }
}

/// <summary>
/// Represents the important statuses for an Correspondence
/// </summary>
public enum CorrespondenceStatus
{
    /// <summary>
    /// Correspondence has been Initialized.
    /// </summary>
    Initialized = 0,

    ///<summary>
    /// Correspondence is ready for publish, but not available for recipient.
    ///</summary>
    ReadyForPublish = 1,

    /// <summary>
    /// Correspondence has been Published, and is available for recipient.
    /// </summary>
    Published = 2,

    /// <summary>
    /// Correspondence fetched by recipient.
    /// </summary>
    Fetched = 3,

    /// <summary>
    /// Correspondence read by recipient.
    /// </summary>
    Read = 4,

    /// <summary>
    /// Recipient has replied to the Correspondence.
    /// </summary>
    Replied = 5,

    /// <summary>
    /// Correspondence has been confirmed by recipient.
    /// </summary>
    Confirmed = 6,

    /// <summary>
    /// Correspondence has been purged by recipient.
    /// </summary>
    PurgedByRecipient = 7,

    /// <summary>
    /// Correspondence has been purged by Altinn.
    /// </summary>
    PurgedByAltinn = 8,

    /// <summary>
    /// Correspondence has been Archived
    /// </summary>
    Archived = 9,

    /// <summary>
    /// Recipient has opted out of digital communication in KRR
    /// </summary>
    Reserved = 10,

    /// <summary>
    /// Correspondence has failed during initialization or processing
    /// </summary>
    Failed = 11,

    /// <summary>
    /// Attachments have been downloaded by recipient
    /// </summary>
    AttachmentsDownloaded = 12,
}

/// <summary>
/// Information about a notification that were created for the correspondence
/// </summary>
public class InitializedCorrespondencesNotifications
{
    /// <summary>
    /// The order ID of the notification
    /// </summary>
    [JsonPropertyName("orderId")]
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Boolean indicating if the notification is a reminder
    /// </summary>
    [JsonPropertyName("isReminder")]
    public bool? IsReminder { get; set; }

    /// <summary>
    /// The status of the notification
    /// </summary>
    [JsonPropertyName("status")]
    public InitializedNotificationStatus Status { get; set; }
}

public enum InitializedNotificationStatus
{
    /// <summary>
    /// The recipient lookup was successful for at least one recipient
    /// </summary>
    Success,

    /// <summary>
    /// The recipient lookup failed for all recipients
    /// </summary>
    MissingContact,

    /// <summary>
    /// The notification order failed to be created due to an error
    /// </summary>
    Failure,
}
