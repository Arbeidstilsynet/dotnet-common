namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

public record InstanceQueryParameters
{
    internal const string AppIdParameterName = "appId";
    internal const string ConfirmedParameterName = "confirmed";
    internal const string ContinuationTokenParameterName = "continuationToken";
    internal const string CreationDateParameterName = "created";
    internal const string CurrentTaskParameterName = "process.currentTask";
    internal const string DueBeforeParameterName = "dueBefore";
    internal const string ExcludeConfirmedByParameterName = "excludeConfirmedBy";
    internal const string InstanceOwnerIdentifierHeaderName = "X-Ai-InstanceOwnerIdentifier";
    internal const string InstanceOwnerPartyIdParameterName = "instanceOwner.partyId";
    internal const string LastChangedParameterName = "lastChanged";
    internal const string MainVersionExcludeParameterName = "mainVersionExclude";
    internal const string MainVersionIncludeParameterName = "mainVersionInclude";
    internal const string OrgParameterName = "org";
    internal const string ProcessEndEventParameterName = "process.endEvent";
    internal const string ProcessEndedParameterName = "process.ended";
    internal const string ProcessIsCompleteParameterName = "process.isComplete";
    internal const string SizeParameterName = "size";
    internal const string StatusIsArchivedParameterName = "status.isArchived";
    internal const string StatusIsHardDeletedParameterName = "status.isHardDeleted";
    internal const string StatusIsSoftDeletedParameterName = "status.isSoftDeleted";
    internal const string VisibleAfterParameterName = "visibleAfter";
    internal const string SearchStringDatabindName = "searchString";
    internal const string SortAscendingDatabindName = "order";

    /// <summary>
    /// The organization identifier.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = OrgParameterName)]
    public string? Org { get; set; }

    /// <summary>
    /// The application identifier.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = AppIdParameterName)]
    public string? AppId { get; set; }

    /// <summary>
    /// The current task identifier.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = CurrentTaskParameterName)]
    public string? ProcessCurrentTask { get; set; }

    /// <summary>
    /// A value indicating whether the process is completed.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = ProcessIsCompleteParameterName)]
    public bool? ProcessIsComplete { get; set; }

    /// <summary>
    /// The process end state.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = ProcessEndEventParameterName)]
    public string ProcessEndEvent { get; set; }

    /// <summary>
    /// The process ended value.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = ProcessEndedParameterName)]
    public AltinnDateTimeQuery[] ProcessEnded { get; set; }

    /// <summary>
    /// The instance owner party identifier.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = InstanceOwnerPartyIdParameterName)]
    public int? InstanceOwnerPartyId { get; set; }

    /// <summary>
    /// The last changed date.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = LastChangedParameterName)]
    public AltinnDateTimeQuery[] LastChanged { get; set; }

    /// <summary>
    /// The creation date.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = CreationDateParameterName)]
    public AltinnDateTimeQuery[] Created { get; set; }

    /// <summary>
    /// The visible after date time.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = VisibleAfterParameterName)]
    public AltinnDateTimeQuery[] VisibleAfter { get; set; }

    /// <summary>
    /// The due before date time.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = DueBeforeParameterName)]
    public AltinnDateTimeQuery[] DueBefore { get; set; }

    /// <summary>
    /// A string that will hide instances already confirmed by stakeholder.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = ExcludeConfirmedByParameterName)]
    public string ExcludeConfirmedBy { get; set; }

    /// <summary>
    /// Confirmed = false is a compact version of ExcludeConfirmedBy indicating
    /// ExcludeConfirmedBy for the org that invokes the request
    /// </summary>
    [MappedQueryParameter(QueryParameterName = ConfirmedParameterName)]
    public bool? Confirmed { get; set; }

    /// <summary>
    /// A value indicating whether the instance is soft deleted.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = StatusIsSoftDeletedParameterName)]
    public bool? IsSoftDeleted { get; set; }

    /// <summary>
    /// A value indicating whether the instance is hard deleted.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = StatusIsHardDeletedParameterName)]
    public bool? IsHardDeleted { get; set; }

    /// <summary>
    /// A value indicating whether the instance is archived.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = StatusIsArchivedParameterName)]
    public bool? IsArchived { get; set; }

    /// <summary>
    /// The continuation token.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = ContinuationTokenParameterName)]
    public string ContinuationToken { get; set; }

    /// <summary>
    /// The page size.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = SizeParameterName)]
    public int? Size { get; set; }

    /// <summary>
    /// The instance owner identifier.
    /// </summary>
    [MappedRequestHeaderParameter(HeaderParameterName = InstanceOwnerIdentifierHeaderName)]
    public string InstanceOwnerIdentifier { get; set; }

    /// <summary>
    /// The Altinn version to include. E.g. "mainVersionInclude=3" will filter the response to only get the Altinn 3 instances.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = MainVersionIncludeParameterName)]
    public int? MainVersionInclude { get; set; }

    /// <summary>
    /// The Altinn version to exclude. E.g. "mainVersionExclude=3" will filter the response to exclude Altinn 3 instances.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = MainVersionExcludeParameterName)]
    public int? MainVersionExclude { get; set; }

    /// <summary>
    /// Gets or sets the search string.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = SearchStringDatabindName)]
    public string SearchString { get; set; }

    /// <summary>
    /// Gets or sets the value by which the result will be sorted.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = SortAscendingDatabindName)]
    public string SortBy { get; set; }
}
