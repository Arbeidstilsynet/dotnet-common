using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;

namespace Arbeidstilsynet.Common.Altinn.Model.Adapter
{
    /// <summary>
    /// Bears all required and optional data to create a correspondence
    /// </summary>
    public class CorrespondenceRequest
    {
        /// <summary>
        /// The Resource Identifier of the resource which should be used. Default is "dat-meldinger-correspondence".
        /// </summary>
        [JsonPropertyName("resourceId")]
        [Required]
        [RegularExpression(
            @"^dat-[a-z0-9\-]+$",
            ErrorMessage = "ResourceIdentifier must start with 'dat-' and contain only lowercase letters, digits, and hyphens."
        )]
        public string? ResourceIdentifier { get; set; } = "dat-meldinger-correspondence";

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
        public required InitializeCorrespondenceContent Content { get; set; }

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
        public Dictionary<string, string> PropertyList { get; set; } =
            new Dictionary<string, string>();

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

        /// <summary>
        /// List of recipients for the correspondence: organization (urn:altinn:organization:identifier-no:ORGNR), national identity number (urn:altinn:person:identifier-no:SSN),
        /// self identified user (urn:altinn:person:idporten-email:EMAIL), or legacy selfidentified user (urn:altinn:person:legacy-selfidentified:USERNAME).
        /// </summary>
        [JsonPropertyName("recipients")]
        [Required]
        public required List<IAltinnRecipient> Recipients { get; set; }

        /// <summary>
        /// Existing attachments that should be added to the correspondence
        /// </summary>
        [JsonPropertyName("existingAttachments")]
        public List<Guid>? ExistingAttachments { get; set; }

        /// <summary>
        /// Optional idempotency key to prevent duplicate correspondence creation
        /// </summary>
        [JsonPropertyName("idempotentKey")]
        public Guid? IdempotentKey { get; set; }
    }
}
