using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/*
** Plain copy of altinn dtos with some reductions.
** Since altinn is not providing a dedicated nuget package only for dtos, we added this to avoid that consumers need to include the complete altinn core package.
*/
namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

/// <summary>
/// Represents a request object for the operation, InitializeCorrespondence, that can create a correspondence in Altinn.
/// </summary>
public class BaseCorrespondence
{
    /// <summary>
    /// The Resource Id associated with the correspondence service.
    /// </summary>
    [JsonPropertyName("resourceId")]
    [StringLength(255, MinimumLength = 1)]
    [Required]
    public required string ResourceId { get; set; }

    /// <summary>
    /// The Sending organization of the correspondence.
    /// </summary>
    /// <remarks>
    /// Organization number must be formatted as countrycode:organizationnumber.
    /// </remarks>
    [JsonPropertyName("sender")]
    [Obsolete(
        "Sender is deprecated and will be removed in a future version. The sender is now automatically determined from the Resource Registry based on the resourceId."
    )]
    public string? Sender { get; set; }

    /// <summary>
    /// A reference used by senders and receivers to identify a specific Correspondence using external identification methods.
    /// </summary>
    [JsonPropertyName("sendersReference")]
    [StringLength(4096, MinimumLength = 1)]
    [Required]
    public required string SendersReference { get; set; }

    /// <summary>
    /// An alternative name for the sender of the correspondence. The name will be displayed instead of the organization name.
    ///  </summary>
    [JsonPropertyName("messageSender")]
    [StringLength(255, MinimumLength = 0)]
    public string? MessageSender { get; set; }

    /// <summary>
    /// The correspondence content. Contains information about the Correspondence body, subject etc.
    /// </summary>
    [JsonPropertyName("content")]
    public InitializeCorrespondenceContent? Content { get; set; }

    /// <summary>
    /// When the correspondence should become visible to the recipient.
    /// </summary>
    [JsonPropertyName("requestedPublishTime")]
    public DateTimeOffset? RequestedPublishTime { get; set; }

    /// <summary>
    /// When the recipient must reply to the correspondence
    /// </summary>
    [JsonPropertyName("dueDateTime")]
    public DateTimeOffset? DueDateTime { get; set; }

    /// <summary>
    /// A list of references Senders can use to tell the recipient that the correspondence is related to the referenced item(s)
    /// Examples include Altinn App instances, Altinn Broker File Transfers
    /// </summary>
    [JsonPropertyName("externalReferences")]
    public List<ExternalReference>? ExternalReferences { get; set; }

    /// <summary>
    /// User-defined properties related to the Correspondence
    /// </summary>
    [JsonPropertyName("propertyList")]
    public Dictionary<string, string> PropertyList { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Options for how the recipient can reply to the Correspondence
    /// </summary>
    [JsonPropertyName("replyOptions")]
    public List<CorrespondenceReplyOption>? ReplyOptions { get; set; } =
        new List<CorrespondenceReplyOption>();

    /// <summary>
    /// Notifications related to the Correspondence.
    /// </summary>
    [JsonPropertyName("notification")]
    public InitializeCorrespondenceNotification? Notification { get; set; }

    /// <summary>
    /// Specifies whether the correspondence can override reservation against digital communication in KRR.
    /// This field only applies to recipients who are persons with person numbers (both default and custom recipients).
    /// It has no effect for organization recipients or email/sms recipients through custom recipients.
    /// </summary>
    [JsonPropertyName("ignoreReservation")]
    public bool? IgnoreReservation { get; set; }

    /// <summary>
    /// Specifies whether reading the correspondence needs to be confirmed by the recipient
    /// </summary>
    [JsonPropertyName("isConfirmationNeeded")]
    public bool IsConfirmationNeeded { get; set; }

    /// <summary>
    /// Specifies whether the correspondence is confidential
    /// </summary>
    [JsonPropertyName("isConfidential")]
    public bool IsConfidential { get; set; }
}

public class BaseAttachment
{
    /// <summary>
    /// The name of the attachment file.
    /// </summary>
    [JsonPropertyName("fileName")]
    [StringLength(255, MinimumLength = 0)]
    public string? FileName { get; set; }

    /// <summary>
    /// A logical name for the file, which will be shown in Altinn Inbox.
    /// </summary>
    [JsonPropertyName("displayName")]
    [StringLength(255, MinimumLength = 0)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// A value indicating whether the attachment is encrypted or not.
    /// </summary>
    [JsonPropertyName("isEncrypted")]
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// MD5 checksum for file data.
    /// </summary>
    [JsonPropertyName("checksum")]
    public string? Checksum { get; set; } = string.Empty;

    /// <summary>
    /// A reference value given to the attachment by the creator.
    /// </summary>
    [JsonPropertyName("sendersReference")]
    [StringLength(4096, MinimumLength = 1)]
    [Required]
    public required string SendersReference { get; set; }

    /// <summary>
    /// Relative expiration time (days) for the attachment.
    /// </summary>
    [JsonPropertyName("expirationInDays")]
    public int? ExpirationInDays { get; set; }
}

/// <summary>
/// Represents a ReplyOption with information provided by the sender.
/// A reply option is a way for recipients to respond to a correspondence in addition to the normal Read and Confirm operations
/// </summary>
public class CorrespondenceReplyOption
{
    /// <summary>
    /// Gets or sets the URL to be used as a reply/response to a correspondence.
    /// </summary>
    [JsonPropertyName("linkURL")]
    public required string LinkURL { get; set; }

    /// <summary>
    /// Gets or sets the url text.
    /// </summary>
    [JsonPropertyName("linkText")]
    public string? LinkText { get; set; }
}

/// <summary>
/// Represents a custom notification recipient with override options
/// </summary>
public class CustomNotificationRecipient
{
    /// <summary>
    /// This is not used, but is required by the API.
    /// </summary>
    [JsonPropertyName("recipientToOverride")]
    public required string RecipientToOverride { get; set; }

    /// <summary>
    /// Only the first recipient will be used as custom recipient.
    /// </summary>
    [JsonPropertyName("recipients")]
    public required List<NotificationRecipient> Recipients { get; set; }
}

/// <summary>
/// Represents a reference to another item in the Altinn ecosystem
/// </summary>
public class ExternalReference
{
    /// <summary>
    /// The Reference Value
    /// </summary>
    [JsonPropertyName("referenceValue")]
    public required string ReferenceValue { get; set; }

    /// <summary>
    /// The Type of reference
    /// </summary>
    [JsonPropertyName("referenceType")]
    public required ReferenceType ReferenceType { get; set; }
}

/// <summary>
/// Represents an attachment to a specific correspondence as part of Initialize Correspondence Operation
/// </summary>
public class InitializeCorrespondenceAttachment : BaseAttachment
{
    /// <summary>
    /// A unique id for the correspondence attachment.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Specifies the location type of the attachment data
    /// </summary>
    [JsonPropertyName("dataLocationType")]
    [Required]
    public InitializeAttachmentDataLocationType DataLocationType { get; set; }
}

/// <summary>
/// Represents the content of a Correspondence.
/// </summary>
public class InitializeCorrespondenceContent
{
    /// <summary>
    /// Gets or sets the language of the correspondence, specified according to ISO 639-1
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets the correspondence message title. Subject.
    /// </summary>
    [JsonPropertyName("messageTitle")]
    public required string MessageTitle { get; set; }

    /// <summary>
    /// Gets or sets a summary text of the correspondence.
    /// </summary>
    [JsonPropertyName("messageSummary")]
    public string? MessageSummary { get; set; }

    /// <summary>
    /// Gets or sets the main body of the correspondence.
    /// </summary>
    [JsonPropertyName("messageBody")]
    public required string MessageBody { get; set; }

    /// <summary>
    /// Gets or sets metadata of the attachments added in the Attachments field. Uses the InitializeCorrespondenceAttachmentExt model.
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<InitializeCorrespondenceAttachment> Attachments { get; set; } =
        new List<InitializeCorrespondenceAttachment>();
}

public class InitializeCorrespondences
{
    /// <summary>
    /// The correspondence object that should be created
    /// </summary>
    [JsonPropertyName("correspondence")]
    public required BaseCorrespondence Correspondence { get; set; }

    /// <summary>
    /// List of recipients for the correspondence: organization (urn:altinn:organization:identifier-no:ORGNR), national identity number (urn:altinn:person:identifier-no:SSN),
    /// self identified user (urn:altinn:person:idporten-email:EMAIL), or legacy selfidentified user (urn:altinn:person:legacy-selfidentified:USERNAME).
    /// </summary>
    [JsonPropertyName("recipients")]
    [Required]
    public required List<string> Recipients { get; set; }

    /// <summary>
    /// Existing attachments that should be added to the correspondence
    /// </summary>
    [JsonPropertyName("existingAttachments")]
    public List<Guid> ExistingAttachments { get; set; } = new List<Guid>();

    /// <summary>
    /// Optional idempotency key to prevent duplicate correspondence creation
    /// </summary>
    [JsonPropertyName("idempotentKey")]
    public Guid? IdempotentKey { get; set; }
}

/// <summary>
/// Used to specify a single notification connected to a specific Correspondence during the Initialize Correspondence operation
/// </summary>
public class InitializeCorrespondenceNotification
{
    /// <summary>
    /// Which of the notification templates to use for this notification
    /// </summary>
    [JsonPropertyName("notificationTemplate")]
    public NotificationTemplate? NotificationTemplate { get; set; }

    /// <summary>
    /// The emails subject for the main notification.
    /// Maximum length is 512 characters.
    /// </summary>
    [JsonPropertyName("emailSubject")]
    [StringLength(512, MinimumLength = 0)]
    public string? EmailSubject { get; set; }

    /// <summary>
    /// The email body for the main notification.
    /// Maximum length is 10,000 characters.
    /// </summary>
    [JsonPropertyName("emailBody")]
    [StringLength(10000, MinimumLength = 0)]
    public string? EmailBody { get; set; }

    /// <summary>
    /// The content type of the email body (HTML or Plain text)
    /// </summary>
    [JsonPropertyName("emailContentType")]
    public EmailContentType EmailContentType { get; set; } = EmailContentType.Plain;

    /// <summary>
    /// The sms body for the main notification.
    /// Maximum length is 2,144 characters (16 SMS segments × 134 characters per segment).
    /// This aligns with the Altinn Notifications service SMS processing limits.
    /// </summary>
    [JsonPropertyName("smsBody")]
    [StringLength(2144, MinimumLength = 0)]
    public string? SmsBody { get; set; }

    /// <summary>
    /// Should a reminder be sent if the notification is not confirmed or opened
    /// </summary>
    [JsonPropertyName("sendReminder")]
    public bool SendReminder { get; set; }

    /// <summary>
    /// The email subject to use for the reminder notification
    /// Maximum length is 512 characters.
    /// </summary>
    [JsonPropertyName("reminderEmailSubject")]
    [StringLength(512, MinimumLength = 0)]
    public string? ReminderEmailSubject { get; set; }

    /// <summary>
    /// The email body to use for the reminder notification.
    /// Maximum length is 10,000 characters.
    /// </summary>
    [JsonPropertyName("reminderEmailBody")]
    [StringLength(10000, MinimumLength = 0)]
    public string? ReminderEmailBody { get; set; }

    /// <summary>
    /// The content type of the reminder email body (HTML or Plain text)
    /// </summary>
    [JsonPropertyName("reminderEmailContentType")]
    public EmailContentType? ReminderEmailContentType { get; set; }

    /// <summary>
    /// The sms body to use for the reminder notification.
    /// Maximum length is 2,144 characters (16 SMS segments × 134 characters per segment).
    /// This aligns with the Altinn Notifications service SMS processing limits.
    /// </summary>
    [JsonPropertyName("reminderSmsBody")]
    [StringLength(2144, MinimumLength = 0)]
    public string? ReminderSmsBody { get; set; }

    /// <summary>
    /// Specifies the notification channel to use for the main notification
    /// </summary>
    [JsonPropertyName("notificationChannel")]
    public NotificationChannel NotificationChannel { get; set; }

    /// <summary>
    ///  Specifies the notification channel to use for the reminder notification
    /// </summary>
    [JsonPropertyName("reminderNotificationChannel")]
    public NotificationChannel? ReminderNotificationChannel { get; set; }

    /// <summary>
    /// Senders Reference for this notification
    /// </summary>
    [JsonPropertyName("sendersReference")]
    public string? SendersReference { get; set; }

    /// <summary>
    /// A list of additional recipients for the notification. These are processed in addition to the Correspondence recipient;
    /// if not set, only the Correspondence recipient receives the notification.
    /// </summary>
    [JsonPropertyName("customRecipients")]
    public List<NotificationRecipient>? CustomRecipients { get; set; }

    /// <summary>
    /// When set to true, only CustomRecipients will be used for notifications, overriding the default correspondence recipient.
    /// This flag can only be used when CustomRecipients is provided.
    /// Default value is false (use default contact info + custom recipients).
    /// </summary>
    [JsonPropertyName("overrideRegisteredContactInformation")]
    public bool OverrideRegisteredContactInformation { get; set; } = false;
}

/// <summary>
/// A class representing a a recipient of a notification
/// </summary>
/// <remarks>
/// External representation to be used in the API.
/// </remarks>
public class NotificationRecipient
{
    /// <summary>
    /// the email address of the recipient
    /// </summary>
    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; set; }

    /// <summary>
    /// the mobileNumber of the recipient
    /// </summary>
    [JsonPropertyName("mobileNumber")]
    public string? MobileNumber { get; set; }

    /// <summary>
    /// the organization number of the recipient
    /// </summary>
    [JsonPropertyName("organizationNumber")]
    public string? OrganizationNumber { get; set; }

    /// <summary>
    /// The SSN of the recipient
    /// </summary>
    [JsonPropertyName("nationalIdentityNumber")]
    public string? NationalIdentityNumber { get; set; }

    /// <summary>
    /// Boolean indicating if the recipient is reserved
    /// </summary>
    [JsonPropertyName("isReserved")]
    public bool? IsReserved { get; set; }
}

public enum EmailContentType
{
    Plain = 0,

    Html = 1,
}

/// <summary>
/// Defines the location of the attachment data during the Initialize Correspondence Operation
/// </summary>
public enum InitializeAttachmentDataLocationType
{
    /// <summary>
    /// Specifies that the attachment data will need to be uploaded to Altinn Correspondence via the Upload Attachment operation
    /// </summary>
    NewCorrespondenceAttachment = 0,

    /// <summary>
    /// Specifies that the attachment  already exist in Altinn Correspondence storage
    /// </summary>
    ExistingCorrespondenceAttachment = 1,

    /// <summary>
    /// Specifies that the attachment data already exist in an external storage
    /// </summary>
    ExistingExternalStorage = 2,
}

/// <summary>
/// Enum describing available notification channels.
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// The selected channel for the notification is only email.
    /// </summary>
    Email = 0,

    /// <summary>
    /// The selected channel for the notification is only sms.
    /// </summary>
    Sms = 1,

    /// <summary>
    /// The selected channel for the notification is email preferred.
    /// </summary>
    EmailPreferred = 2,

    /// <summary>
    /// The selected channel for the notification is SMS preferred.
    /// </summary>
    SmsPreferred = 3,

    /// <summary>
    /// The selected channel for the notification is both email and sms.
    /// </summary>
    EmailAndSms = 4,
}

/// <summary>
/// Enum describing available notification templates.
/// </summary>
public enum NotificationTemplate
{
    /// <summary>
    /// Fully customizable template.
    /// </summary>
    CustomMessage,

    /// <summary>
    /// Standard Altinn notification template.
    /// </summary>
    GenericAltinnMessage,
}

/// <summary>
/// Defines what kind of reference
/// </summary>
public enum ReferenceType
{
    /// <summary>
    /// Specifies a generic reference
    /// </summary>
    Generic = 0,

    /// <summary>
    /// Specifies that the reference is to a Altinn App Instance
    /// </summary>
    AltinnAppInstance = 1,

    /// <summary>
    /// Specifies that the reference is to a Altinn Broker File Transfer
    /// </summary>
    AltinnBrokerFileTransfer = 2,

    /// <summary>
    /// Specifies that the reference is a Dialogporten Dialog ID
    /// </summary>
    DialogportenDialogId = 3,

    /// <summary>
    /// Specifies that the reference is a Dialogporten Process ID
    /// </summary>
    DialogportenProcessId = 4,

    /// <summary>
    /// Specifies that the reference is a Dialogporten Transmission ID
    /// </summary>
    DialogportenTransmissionId = 5,

    /// <summary>
    /// Specifies that the reference is a Dialogporten Transmission Type
    /// </summary>
    DialogportenTransmissionType = 6,

    /// <summary>
    /// Specifies that the reference is a Dialogporten Dialog Status
    /// </summary>
    DialogportenDialogStatus = 7,

    /// <summary>
    /// Specifies that the reference is a Dialogporten Dialog Extended Status.
    /// The corresponding referenceValue must be 25 characters or fewer.
    /// </summary>
    DialogportenDialogExtendedStatus = 8,
}
