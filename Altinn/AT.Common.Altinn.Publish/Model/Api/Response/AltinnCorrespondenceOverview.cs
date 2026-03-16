using System.Text.Json.Serialization;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;

/*
** Plain copy of altinn dtos with some reductions.
** Since altinn is not providing a dedicated nuget package only for dtos, we added this to avoid that consumers need to include the complete altinn core package.
*/
namespace Arbeidstilsynet.Common.Altinn.Model.Api.Response;

/// <summary>
/// An object representing an overview of a correspondence with enough details to drive the business process
/// </summary>
public class AltinnCorrespondenceOverview : BaseCorrespondence
{
    /// <summary>
    /// The recipient of the correspondence.
    /// </summary>
    [JsonPropertyName("recipient")]
    public required string Recipient { get; set; }

    /// <summary>
    /// Unique Id for this correspondence
    /// </summary>
    [JsonPropertyName("correspondenceId")]
    public required Guid CorrespondenceId { get; set; }

    /// <summary>
    /// The correspondence content. Contains information about the Correspondence body, subject etc.
    /// </summary>
    [JsonPropertyName("content")]
    public new CorrespondenceContent? Content { get; set; }

    /// <summary>
    /// When the correspondence was created
    /// </summary>
    [JsonPropertyName("created")]
    public required DateTimeOffset Created { get; set; }

    /// <summary>
    /// The current status for the Correspondence
    /// </summary>
    [JsonPropertyName("status")]
    public CorrespondenceStatus Status { get; set; }

    /// <summary>
    /// The current status text for the Correspondence
    /// </summary>
    [JsonPropertyName("statusText")]
    public string StatusText { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp for when the Current Correspondence Status was changed
    /// </summary>
    [JsonPropertyName("statusChanged")]
    public DateTimeOffset StatusChanged { get; set; }

    /// <summary>
    /// An overview of the notifications for this correspondence
    /// </summary>
    [JsonPropertyName("notifications")]
    public List<CorrespondenceNotificationOverview> Notifications { get; set; } =
        new List<CorrespondenceNotificationOverview>();

    /// <summary>
    /// The identifier/reference from Altinn 2 for migrated correspondence. Will be null for correspondence created in Altinn 3.
    /// </summary>
    [JsonPropertyName("altinn2CorrespondenceId")]
    public int? Altinn2CorrespondenceId { get; set; }

    /// <summary>
    /// Is null until the correspondence is published.
    /// </summary>
    [JsonPropertyName("published")]
    public DateTimeOffset? Published { get; set; }
}

/// <summary>
/// Represents the content of a reportee element of the type correspondence.
/// </summary>
public class CorrespondenceContent : InitializeCorrespondenceContent
{
    /// <summary>
    /// Gets or sets a list of attachments.
    /// </summary>
    [JsonPropertyName("attachments")]
    public new required List<CorrespondenceAttachment> Attachments { get; set; }
}

/// <summary>
/// Represents a binary attachment to a Correspondence
/// </summary>
public class CorrespondenceAttachment : InitializeCorrespondenceAttachment
{
    /// <summary>
    /// The date on which this attachment is created
    /// </summary>
    [JsonPropertyName("created")]
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Specifies the location of the attachment data
    /// </summary>
    [JsonPropertyName("dataLocationType")]
    public new AttachmentDataLocationType DataLocationType { get; set; }

    /// <summary>
    /// Current attachment status
    /// </summary>
    [JsonPropertyName("status")]
    public AttachmentStatus Status { get; set; }

    /// <summary>
    /// Current attachment status text description
    /// </summary>
    [JsonPropertyName("statusText")]
    public string StatusText { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp for when the Current Attachment Status was changed
    /// </summary>
    [JsonPropertyName("statusChanged")]
    public DateTimeOffset StatusChanged { get; set; }

    /// <summary>
    /// The attachment data type in MIME format
    /// </summary>
    [JsonPropertyName("dataType")]
    public string DataType { get; set; }

    /// <summary>
    /// The expiration time for this attachment on this correspondence.
    /// </summary>
    [JsonPropertyName("expirationTime")]
    public DateTimeOffset? ExpirationTime { get; set; }
}

public class CorrespondenceNotificationOverview
{
    public Guid? NotificationOrderId { get; set; }
    public bool IsReminder { get; set; }
}

/// <summary>
/// Represents the important statuses for an attachment
/// </summary>
public enum AttachmentStatus
{
    /// <summary>
    /// Attachment has been Initialized.
    /// </summary>
    Initialized = 0,

    /// <summary>
    /// Attachment is awaiting processing of upload
    /// </summary>
    UploadProcessing = 1,

    /// <summary>
    /// Attachment is published and available for download
    /// </summary>
    Published = 2,

    /// <summary>
    /// Attachment has been purged
    /// </summary>
    Purged = 3,

    /// <summary>
    /// Attachment has failed during processing
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Attachment has expired
    /// </summary>
    Expired = 5,
}

/// <summary>
/// Defines the location of the attachment data
/// </summary>
public enum AttachmentDataLocationType
{
    /// <summary>
    /// Specifies that the attachment data is stored in the Altinn Correspondence Storage
    /// </summary>
    AltinnCorrespondenceAttachment = 0,

    /// <summary>
    /// Specifies that the attachment data is stored in an external storage controlled by the sender
    /// </summary>
    ExternalStorage = 1,
}
