using System.Text.Json.Serialization;

/*
** Plain copy of altinn dtos with some reductions.
** Since altinn is not providing a dedicated nuget package only for dtos, we added this to avoid that consumers need to include the complete altinn core package.
**
** Nullability mirrors the Altinn OpenAPI specifications, which mark these properties optional, so
** anything Altinn may omit is nullable here too.
*/
namespace Arbeidstilsynet.Common.Altinn.Model.Api.Response
{
    /// <summary>
    /// Represents an Altinn instance. Colloquially known as "Altinn-skjema".
    /// </summary>
    public record AltinnInstance
    {
        /// <summary>
        /// Gets the unique id of the instance {instanceOwnerId}/{instanceGuid}.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>
        /// Gets the instance owner information.
        /// </summary>
        [JsonPropertyName("instanceOwner")]
        public InstanceOwner? InstanceOwner { get; init; }

        /// <summary>
        /// Gets the id of the application this is an instance of, e.g. {org}/{app22}.
        /// </summary>
        [JsonPropertyName("appId")]
        public string? AppId { get; init; }

        /// <summary>
        /// Gets application owner identifier, usually a abbreviation of organisation name. All in lower case.
        /// </summary>
        [JsonPropertyName("org")]
        public string? Org { get; init; }

        /// <summary>
        /// Gets an object containing the instance process state.
        /// </summary>
        [JsonPropertyName("process")]
        public ProcessState? Process { get; init; }

        /// <summary>
        /// Gets a list of <see cref="CompleteConfirmation"/> elements.
        /// </summary>
        [JsonPropertyName("completeConfirmations")]
        public List<CompleteConfirmation>? CompleteConfirmations { get; init; }

        /// <summary>
        /// Gets a list of data elements associated with the instance
        /// </summary>
        [JsonPropertyName("data")]
        public List<DataElement>? Data { get; init; }

        /// <summary>
        /// Gets the data values for the instance. Never null; an omitted value yields an empty dictionary.
        /// </summary>
        [JsonPropertyName("dataValues")]
        public Dictionary<string, string> DataValues
        {
            get => _dataValues;
            init => _dataValues = value ?? [];
        }

        private readonly Dictionary<string, string> _dataValues = [];
    }

    /// <summary>
    /// Represents the owner of an Altinn instance. This is usually the user that sent the "Altinn-skjema" (or the organization they did it on behalf of).
    /// </summary>
    public record InstanceOwner
    {
        /// <summary>
        /// Gets the party id of the instance owner (also called instance owner party id).
        /// </summary>
        [JsonPropertyName("partyId")]
        public string? PartyId { get; init; }

        /// <summary>
        /// Gets person number (national identification number) of the party. Null if the party is not a person.
        /// </summary>
        [JsonPropertyName("personNumber")]
        public string? PersonNumber { get; init; }

        /// <summary>
        /// Gets the organisation number of the party. Null if the party is not an organisation.
        /// </summary>
        [JsonPropertyName("organisationNumber")]
        public string? OrganisationNumber { get; init; }

        /// <summary>
        /// Gets the username of the party. Null if the party is not self identified.
        /// </summary>
        [JsonPropertyName("username")]
        public string? Username { get; init; }
    }

    /// <summary>
    /// Represents the process state of an Altinn instance.
    /// </summary>
    public record ProcessState
    {
        /// <summary>
        /// Gets the date and time for when the process was started.
        /// </summary>
        [JsonPropertyName("started")]
        public DateTime? Started { get; init; }

        /// <summary>
        /// Gets the event that was used to start the process.
        /// </summary>
        [JsonPropertyName("startEvent")]
        public string? StartEvent { get; init; }

        /// <summary>
        /// Gets the date and time for then the process ended/completed.
        /// </summary>
        [JsonPropertyName("ended")]
        public DateTime? Ended { get; init; }

        /// <summary>
        /// Gets the end event of the process.
        /// </summary>
        [JsonPropertyName("endEvent")]
        public string? EndEvent { get; init; }
    }

    /// <summary>
    /// Represents a data element (file) associated with an Altinn instance. This can be the data model itself or attachments uploaded by the user.
    /// </summary>
    public record DataElement
    {
        /// <summary>
        /// Gets the unique id, a guid.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>
        /// Gets the id of the instance which the data element belongs to.
        /// This field is normally not populated if data element is part of instance metadata.
        /// </summary>
        [JsonPropertyName("instanceGuid")]
        public string? InstanceGuid { get; init; }

        /// <summary>
        /// Gets the data type, must be equal to the ones defined in application data types.
        /// </summary>
        [JsonPropertyName("dataType")]
        public string? DataType { get; init; }

        /// <summary>
        /// Gets the name of the data element (file)
        /// </summary>
        [JsonPropertyName("filename")]
        public string? Filename { get; init; }

        /// <summary>
        /// Gets the content type in the stored data element (file).
        /// </summary>
        [JsonPropertyName("contentType")]
        public string? ContentType { get; init; }

        /// <summary>
        /// Gets the size of file in bytes
        /// </summary>
        [JsonPropertyName("size")]
        public long? Size { get; init; }

        /// <summary>
        /// Gets the computed MD5 hash value of the blob. (Base64 encoded string, not the more common hex encoding)
        /// </summary>
        [JsonPropertyName("contentHash")]
        public string? ContentHash { get; init; }

        /// <summary>
        /// Gets a value indicating whether the element has been read.
        /// </summary>
        [JsonPropertyName("isRead")]
        public bool? IsRead { get; init; } = true;

        /// <summary>
        /// Gets a collection of tags associated with the data element.
        /// </summary>
        [JsonPropertyName("tags")]
        public List<string> Tags { get; init; } = [];

        /// <summary>
        /// Gets user-defined metadata associated with the data element.
        /// </summary>
        /// <remarks>
        /// Changeable by the end user, like tags, and is not suitable to store system-controlled metadata.
        /// </remarks>
        [JsonPropertyName("userDefinedMetadata")]
        public Dictionary<string, string> UserDefinedMetadata { get; init; } = [];

        /// <summary>
        /// Gets application-defined metadata associated with the data element.
        /// </summary>
        /// <remarks>
        ///  Meant to be used in custom backend code. This field should not be changeable by the end user.
        /// </remarks>
        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; init; } = [];

        /// <summary>
        /// Gets the result of a file scan of the blob represented by this data element.
        /// </summary>
        [JsonPropertyName("fileScanResult")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FileScanResult? FileScanResult { get; init; }
    }

    /// <summary>
    /// The result of scanning the file behind a data element for malware.
    /// </summary>
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

    /// <summary>
    /// Represents a stakeholder's confirmation that an instance has been received.
    /// </summary>
    public record CompleteConfirmation
    {
        /// <summary>
        /// Gets a unique identifier for a stakeholder.
        /// </summary>
        [JsonPropertyName("stakeholderId")]
        public string? StakeholderId { get; init; }

        /// <summary>
        /// Gets the date and time for when the complete confirmation was created.
        /// </summary>
        [JsonPropertyName("confirmedOn")]
        public DateTime? ConfirmedOn { get; init; }
    }
}
