namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

/// <summary>
/// Defines filters and paging options for an Altinn instance query.
/// </summary>
public record InstanceQueryParameters
{
    internal const string ContinuationTokenParameterName = "continuationToken";

    /// <summary>
    /// The header Altinn expects the instance owner identifier in.
    /// </summary>
    /// <remarks>
    /// Kiota omits header parameters from the generated query-parameter class, so this is applied
    /// to the request directly.
    /// </remarks>
    internal const string InstanceOwnerIdentifierHeaderName = "X-Ai-InstanceOwnerIdentifier";

    /// <summary>
    /// The organization identifier.
    /// </summary>
    public string? Org { get; init; }

    /// <summary>
    /// The application identifier.
    /// </summary>
    public string? AppId { get; init; }

    /// <summary>
    /// The current task identifier.
    /// </summary>
    public string? ProcessCurrentTask { get; init; }

    /// <summary>
    /// A value indicating whether the process is completed.
    /// </summary>
    public bool? ProcessIsComplete { get; init; }

    /// <summary>
    /// The process end state.
    /// </summary>
    public string? ProcessEndEvent { get; init; }

    /// <summary>
    /// The process ended value.
    /// </summary>
    public AltinnDateTimeQuery[]? ProcessEnded { get; init; }

    /// <summary>
    /// The instance owner party identifier.
    /// </summary>
    public int? InstanceOwnerPartyId { get; init; }

    /// <summary>
    /// The last changed date.
    /// </summary>
    public AltinnDateTimeQuery[]? LastChanged { get; init; }

    /// <summary>
    /// The creation date.
    /// </summary>
    public AltinnDateTimeQuery[]? Created { get; init; }

    /// <summary>
    /// The visible after date time.
    /// </summary>
    public AltinnDateTimeQuery[]? VisibleAfter { get; init; }

    /// <summary>
    /// The due before date time.
    /// </summary>
    public AltinnDateTimeQuery[]? DueBefore { get; init; }

    /// <summary>
    /// A string that will hide instances already confirmed by stakeholder.
    /// </summary>
    public string? ExcludeConfirmedBy { get; init; }

    /// <summary>
    /// Confirmed = false is a compact version of ExcludeConfirmedBy indicating
    /// ExcludeConfirmedBy for the org that invokes the request
    /// </summary>
    public bool? Confirmed { get; init; }

    /// <summary>
    /// A value indicating whether the instance is soft deleted.
    /// </summary>
    public bool? IsSoftDeleted { get; init; }

    /// <summary>
    /// A value indicating whether the instance is hard deleted.
    /// </summary>
    public bool? IsHardDeleted { get; init; }

    /// <summary>
    /// A value indicating whether the instance is archived.
    /// </summary>
    public bool? IsArchived { get; init; }

    /// <summary>
    /// The continuation token.
    /// </summary>
    public string? ContinuationToken { get; init; }

    /// <summary>
    /// The page size.
    /// </summary>
    public int? Size { get; init; }

    /// <summary>
    /// The instance owner identifier.
    /// </summary>
    public string? InstanceOwnerIdentifier { get; init; }

    /// <summary>
    /// The Altinn version to include. E.g. "mainVersionInclude=3" will filter the response to only get the Altinn 3 instances.
    /// </summary>
    public int? MainVersionInclude { get; init; }

    /// <summary>
    /// The Altinn version to exclude. E.g. "mainVersionExclude=3" will filter the response to exclude Altinn 3 instances.
    /// </summary>
    public int? MainVersionExclude { get; init; }

    /// <summary>
    /// Gets or sets the search string.
    /// </summary>
    public string? SearchString { get; init; }

    /// <summary>
    /// Gets or sets the value by which the result will be sorted.
    /// </summary>
    public string? SortBy { get; init; }
}
