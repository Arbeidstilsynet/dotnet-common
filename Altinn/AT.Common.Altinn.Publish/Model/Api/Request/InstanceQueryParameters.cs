namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

public class InstanceQueryParameters
{
    private const string _appIdParameterName = "appId";
    private const string _confirmedParameterName = "confirmed";
    private const string _continuationTokenParameterName = "continuationToken";
    private const string _creationDateParameterName = "created";
    private const string _currentTaskParameterName = "process.currentTask";
    private const string _dueBeforeParameterName = "dueBefore";
    private const string _excludeConfirmedByParameterName = "excludeConfirmedBy";
    private const string _instanceOwnerIdentifierHeaderName = "X-Ai-InstanceOwnerIdentifier";
    private const string _instanceOwnerPartyIdParameterName = "instanceOwner.partyId";
    private const string _lastChangedParameterName = "lastChanged";
    private const string _mainVersionExcludeParameterName = "mainVersionExclude";
    private const string _mainVersionIncludeParameterName = "mainVersionInclude";
    private const string _orgParameterName = "org";
    private const string _processEndEventParameterName = "process.endEvent";
    private const string _processEndedParameterName = "process.ended";
    private const string _processIsCompleteParameterName = "process.isComplete";
    private const string _sizeParameterName = "size";
    private const string _statusIsArchivedParameterName = "status.isArchived";
    private const string _statusIsHardDeletedParameterName = "status.isHardDeleted";
    private const string _statusIsSoftDeletedParameterName = "status.isSoftDeleted";
    private const string _visibleAfterParameterName = "visibleAfter";
    private const string _searchStringDatabindName = "searchString";
    private const string _sortAscendingDatabindName = "order";

    /// <summary>
    /// The organization identifier.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _orgParameterName)]
    public string? Org { get; set; }

    /// <summary>
    /// The application identifier.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _appIdParameterName)]
    public string? AppId { get; set; }

    /// <summary>
    /// The current task identifier.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _currentTaskParameterName)]
    public string? ProcessCurrentTask { get; set; }

    /// <summary>
    /// A value indicating whether the process is completed.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _processIsCompleteParameterName)]
    public bool? ProcessIsComplete { get; set; }

    /// <summary>
    /// The process end state.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _processEndEventParameterName)]
    public string ProcessEndEvent { get; set; }

    /// <summary>
    /// The process ended value.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _processEndedParameterName)]
    public AltinnDateTimeQuery[] ProcessEnded { get; set; }

    /// <summary>
    /// The instance owner party identifier.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _instanceOwnerPartyIdParameterName)]
    public int? InstanceOwnerPartyId { get; set; }

    /// <summary>
    /// The last changed date.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _lastChangedParameterName)]
    public AltinnDateTimeQuery[] LastChanged { get; set; }

    /// <summary>
    /// The creation date.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _creationDateParameterName)]
    public AltinnDateTimeQuery[] Created { get; set; }

    /// <summary>
    /// The visible after date time.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _visibleAfterParameterName)]
    public AltinnDateTimeQuery[] VisibleAfter { get; set; }

    /// <summary>
    /// The due before date time.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _dueBeforeParameterName)]
    public AltinnDateTimeQuery[] DueBefore { get; set; }

    /// <summary>
    /// A string that will hide instances already confirmed by stakeholder.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _excludeConfirmedByParameterName)]
    public string ExcludeConfirmedBy { get; set; }

    /// <summary>
    /// Confirmed = false is a compact version of ExcludeConfirmedBy indicating
    /// ExcludeConfirmedBy for the org that invokes the request
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _confirmedParameterName)]
    public bool? Confirmed { get; set; }

    /// <summary>
    /// A value indicating whether the instance is soft deleted.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _statusIsSoftDeletedParameterName)]
    public bool? IsSoftDeleted { get; set; }

    /// <summary>
    /// A value indicating whether the instance is hard deleted.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _statusIsHardDeletedParameterName)]
    public bool? IsHardDeleted { get; set; }

    /// <summary>
    /// A value indicating whether the instance is archived.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _statusIsArchivedParameterName)]
    public bool? IsArchived { get; set; }

    /// <summary>
    /// The continuation token.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _continuationTokenParameterName)]
    public string ContinuationToken { get; set; }

    /// <summary>
    /// The page size.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _sizeParameterName)]
    public int? Size { get; set; }

    /// <summary>
    /// The instance owner identifier.
    /// </summary>
    [MappedRequestHeaderParameter(
        HeaderParameterName = _instanceOwnerIdentifierHeaderName
    )]
    public string InstanceOwnerIdentifier { get; set; }

    /// <summary>
    /// The Altinn version to include. E.g. "mainVersionInclude=3" will filter the response to only get the Altinn 3 instances.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _mainVersionIncludeParameterName)]
    public int? MainVersionInclude { get; set; }

    /// <summary>
    /// The Altinn version to exclude. E.g. "mainVersionExclude=3" will filter the response to exclude Altinn 3 instances.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _mainVersionExcludeParameterName)]
    public int? MainVersionExclude { get; set; }

    /// <summary>
    /// Gets or sets the search string.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _searchStringDatabindName)]
    public string SearchString { get; set; }

    /// <summary>
    /// Gets or sets the value by which the result will be sorted.
    /// </summary>
    [MappedQueryParameter(QueryParameterName = _sortAscendingDatabindName)]
    public string SortBy { get; set; }
}
