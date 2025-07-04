namespace Arbeidstilsynet.Common.GeoNorge.Model.Response;

/// <summary>
/// Result of a paginated query containing elements of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of elements contained in the pagination result.</typeparam>
public record PaginationResult<T>
{
    /// <summary>
    /// Total number of elements in the result set, regardless of pagination.
    /// </summary>
    public long TotalElements { get; init; }

    /// <summary>
    /// Number of elements per page.
    /// </summary>
    public long PageSize { get; init; }

    /// <summary>
    /// 0-based page index.
    /// </summary>
    public long PageIndex { get; init; }

    /// <summary>
    /// Elements on this page.
    /// </summary>
    public IEnumerable<T> Elements { get; init; } = [];
}
