using System.Text.Json.Serialization;

/*
** Plain copy of altinn dtos with some reductions.
** Since altinn is not providing a dedicated nuget package only for dtos, we added this to avoid that consumers need to include the complete altinn core package.
*/
namespace Arbeidstilsynet.Common.Altinn.Model.Api.Response
{
    /// <summary>
    /// Represents an Altinn instance. Colloquially known as "Altinn-skjema".
    /// </summary>
    public class AltinnInstance
    {
        /// <summary>
        /// Gets or sets the unique id of the instance {instanceOwnerId}/{instanceGuid}.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the instance owner information.
        /// </summary>
        [JsonPropertyName("instanceOwner")]
        public InstanceOwner InstanceOwner { get; set; }

        /// <summary>
        /// Gets or sets the id of the application this is an instance of, e.g. {org}/{app22}.
        /// </summary>
        [JsonPropertyName("appId")]
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets application owner identifier, usually a abbreviation of organisation name. All in lower case.
        /// </summary>
        [JsonPropertyName("org")]
        public string Org { get; set; }

        /// <summary>
        /// Gets or sets a set of URLs to access the instance metadata resource.
        /// </summary>
        [JsonPropertyName("selfLinks")]
        public ResourceLinks SelfLinks { get; set; }

        /// <summary>
        /// Gets or sets the due date to submit the instance to application owner.
        /// </summary>
        [JsonPropertyName("dueBefore")]
        public DateTime? DueBefore { get; set; }

        /// <summary>
        /// Gets or sets date and time for when the instance should first become visible for the instance owner.
        /// </summary>
        [JsonPropertyName("visibleAfter")]
        public DateTime? VisibleAfter { get; set; }

        /// <summary>
        /// Gets or sets an object containing the instance process state.
        /// </summary>
        [JsonPropertyName("process")]
        public ProcessState Process { get; set; }

        /// <summary>
        /// Gets or sets the type of finished status of the instance.
        /// </summary>
        [JsonPropertyName("status")]
        public InstanceStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a list of <see cref="CompleteConfirmation"/> elements.
        /// </summary>
        [JsonPropertyName("completeConfirmations")]
        public List<CompleteConfirmation> CompleteConfirmations { get; set; }

        /// <summary>
        /// Gets or sets a list of data elements associated with the instance
        /// </summary>
        [JsonPropertyName("data")]
        public List<DataElement> Data { get; set; }

        /// <summary>
        /// Gets or sets the presentation texts for the instance.
        /// </summary>
        [JsonPropertyName("presentationTexts")]
        public Dictionary<string, string> PresentationTexts { get; set; }

        /// <summary>
        /// Gets or sets the data values for the instance.
        /// </summary>
        [JsonPropertyName("dataValues")]
        public Dictionary<string, string> DataValues { get; set; }
    }

    /// <summary>
    /// Represents the owner of an Altinn instance. This is usually the user that sent the "Altinn-skjema" (or the organization they did it on behalf of).
    /// </summary>
    public class InstanceOwner
    {
        /// <summary>
        /// Gets or sets the party id of the instance owner (also called instance owner party id).
        /// </summary>
        [JsonPropertyName("partyId")]
        public string PartyId { get; set; }

        /// <summary>
        /// Gets or sets person number (national identification number) of the party. Null if the party is not a person.
        /// </summary>
        [JsonPropertyName("personNumber")]
        public string PersonNumber { get; set; }

        /// <summary>
        /// Gets or sets the organisation number of the party. Null if the party is not an organisation.
        /// </summary>
        [JsonPropertyName("organisationNumber")]
        public string OrganisationNumber { get; set; }

        /// <summary>
        /// Gets or sets the username of the party. Null if the party is not self identified.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; }
    }

    public class ResourceLinks
    {
        /// <summary>
        /// Gets or sets the application resource link. It is null if data is fetched from platform storage.
        /// </summary>
        [JsonPropertyName("apps")]
        public string Apps { get; set; }

        /// <summary>
        /// Gets or sets platform resource link.
        /// </summary>
        [JsonPropertyName("platform")]
        public string Platform { get; set; }
    }

    public class ProcessState
    {
        /// <summary>
        /// Gets or sets the date and time for when the process was started.
        /// </summary>
        [JsonPropertyName("started")]
        public DateTime? Started { get; set; }

        /// <summary>
        /// Gets or sets the event that was used to start the process.
        /// </summary>
        [JsonPropertyName("startEvent")]
        public string StartEvent { get; set; }

        /// <summary>
        /// Gets or sets the date and time for then the process ended/completed.
        /// </summary>
        [JsonPropertyName("ended")]
        public DateTime? Ended { get; set; }

        /// <summary>
        /// Gets or sets the end event of the process.
        /// </summary>
        [JsonPropertyName("endEvent")]
        public string EndEvent { get; set; }
    }

    /// <summary>
    /// Represents a data element (file) associated with an Altinn instance. This can be the data model itself or attachments uploaded by the user.
    /// </summary>
    public class DataElement
    {
        /// <summary>
        /// Gets or sets the unique id, a guid.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the id of the instance which the data element belongs to.
        /// This field is normally not populated if data element is part of instance metadata.
        /// </summary>
        [JsonPropertyName("instanceGuid")]
        public string InstanceGuid { get; set; }

        /// <summary>
        /// Gets or sets the data type, must be equal to the ones defined in application data types.
        /// </summary>
        [JsonPropertyName("dataType")]
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the name of the data element (file)
        /// </summary>
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the content type in the stored data element (file).
        /// </summary>
        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the path to blob storage. Might be nullified in export.
        /// </summary>
        [JsonPropertyName("blobStoragePath")]
        public string BlobStoragePath { get; set; }

        /// <summary>
        /// Gets or sets links to access the data element.
        /// </summary>
        [JsonPropertyName("selfLinks")]
        public ResourceLinks SelfLinks { get; set; }

        /// <summary>
        /// Gets or sets the size of file in bytes
        /// </summary>
        [JsonPropertyName("size")]
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the computed MD5 hash value of the blob. (Base64 encoded string, not the more common hex encoding)
        /// </summary>
        [JsonPropertyName("contentHash")]
        public string ContentHash { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the element can be updated.
        /// </summary>
        [JsonPropertyName("locked")]
        public bool Locked { get; set; }

        /// <summary>
        /// Gets or sets an optional array of data element references.
        /// </summary>
        [JsonPropertyName("refs")]
        public List<Guid> Refs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the element has been read.
        /// </summary>
        [JsonPropertyName("isRead")]
        public bool IsRead { get; set; } = true;

        /// <summary>
        /// Gets or sets a collection of tags associated with the data element.
        /// </summary>
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets user-defined metadata associated with the data element.
        /// </summary>
        /// <remarks>
        /// Changeable by the end user, like tags, and is not suitable to store system-controlled metadata.
        /// </remarks>
        [JsonPropertyName("userDefinedMetadata")]
        public List<KeyValueEntry> UserDefinedMetadata { get; set; }

        /// <summary>
        /// Gets or sets application-defined metadata associated with the data element.
        /// </summary>
        /// <remarks>
        ///  Meant to be used in custom backend code. This field should not be changeable by the end user.
        /// </remarks>
        [JsonPropertyName("metadata")]
        public List<KeyValueEntry> Metadata { get; set; }

        /// <summary>
        /// Gets or sets the delete status of the data element.
        /// </summary>
        [JsonPropertyName("deleteStatus")]
        public DeleteStatus DeleteStatus { get; set; }

        /// <summary>
        /// Gets or sets the result of a file scan of the blob represented by this data element.
        /// </summary>
        [JsonPropertyName("fileScanResult")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FileScanResult FileScanResult { get; set; }
    }

    public class DataElementList
    {
        /// <summary>
        /// The actual list of data elements.
        /// </summary>
        [JsonPropertyName("dataElements")]
        public List<DataElement> DataElements { get; set; }
    }

    /// <summary>
    /// Represents a key-value pair.
    /// </summary>
    public class KeyValueEntry
    {
        /// <summary>
        /// The key. Must be unique within the list.
        /// </summary>
        [JsonPropertyName("key")]
        public string Key { get; set; }

        /// <summary>
        /// The value.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class InstanceStatus
    {
        /// <summary>
        /// Gets or sets if the instance is archived.
        /// </summary>
        [JsonPropertyName("isArchived")]
        public bool IsArchived { get; set; }

        /// <summary>
        /// Gets or sets the date the instance was archived.
        /// </summary>
        [JsonPropertyName("archived")]
        public DateTime? Archived { get; set; }

        /// <summary>
        /// Gets or sets if the instance is soft deleted.
        /// </summary>
        [JsonPropertyName("isSoftDeleted")]
        public bool IsSoftDeleted { get; set; }

        /// <summary>
        /// Gets or sets the date the instance was deleted.
        /// </summary>
        [JsonPropertyName("softDeleted")]
        public DateTime? SoftDeleted { get; set; }

        /// <summary>
        /// Gets or sets if the instance is hard deleted.
        /// </summary>
        [JsonPropertyName("isHardDeleted")]
        public bool IsHardDeleted { get; set; }

        /// <summary>
        /// Gets or sets the date the instance was marked for hard delete by user.
        /// </summary>
        [JsonPropertyName("hardDeleted")]
        public DateTime? HardDeleted { get; set; }

        /// <summary>
        /// Gets or sets the read status of the instance.
        /// </summary>
        [JsonPropertyName("readStatus")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReadStatus ReadStatus { get; set; }

        /// <summary>
        /// Gets or sets the sub status of the instance.
        /// </summary>
        [JsonPropertyName("substatus")]
        public Substatus Substatus { get; set; }
    }

    /// <summary>
    /// The read status
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReadStatus
    {
        /// <summary>
        /// Instance is unread
        /// </summary>
        Unread,

        /// <summary>
        /// Instance is read
        /// </summary>
        Read,

        /// <summary>
        /// Instance has been updated since last review
        /// </summary>
        UpdatedSinceLastReview,
    }

    /// <summary>
    /// The substatus
    /// </summary>
    public class Substatus
    {
        /// <summary>
        /// A text key pointing to a short description of the substatus.
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; set; }

        /// <summary>
        /// A text key pointing to a longer description of the substatus.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FileScanResult
    {
        /// <summary>
        /// The file will not be scanned. File scanning is turned off.
        /// </summary>
        NotApplicable,

        /// <summary>
        /// The scan status of the file is pending. This is the default value.
        /// </summary>
        Pending,

        /// <summary>
        /// The file scan did not find any malware in the file.
        /// </summary>
        Clean,

        /// <summary>
        /// The file scan found malware in the file.
        /// </summary>
        Infected,
    }

    public class DeleteStatus
    {
        /// <summary>
        /// Gets or sets if the element is hard deleted.
        /// </summary>
        [JsonPropertyName("isHardDeleted")]
        public bool IsHardDeleted { get; set; }

        /// <summary>
        /// Gets or sets the date the element was marked for hard delete.
        /// </summary>
        [JsonPropertyName("hardDeleted")]
        public DateTime? HardDeleted { get; set; }
    }

    public class CompleteConfirmation
    {
        /// <summary>
        /// Gets or sets a unique identifier for a stakeholder.
        /// </summary>
        [JsonPropertyName("stakeholderId")]
        public string StakeholderId { get; set; }

        /// <summary>
        /// Gets or sets the date and time for when the complete confirmation was created.
        /// </summary>
        [JsonPropertyName("confirmedOn")]
        public DateTime ConfirmedOn { get; set; }
    }
}
