namespace Arbeidstilsynet.Common.GeoNorge.Model.Request;

/// <summary>
/// Represents pagination parameters for queries.
/// </summary>
public record Pagination
{
    /// <summary>
    /// 0-based page index.
    /// </summary>
    public long PageIndex { get; init; } = 0;
    
    /// <summary>
    /// Number of elements per page. Default is 10.
    /// </summary>
    public long PageSize { get; init; } = 10;
}