namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;

/// <summary>
/// Represents pagination and sorting for searches in the Enhetsregisteret.
/// </summary>
/// <remarks>
/// The search max result size for Enhetsregisteret/Brreg is 10_000. Any page extent (page+1 * size) exceeding this will throw an exception.
/// </remarks>
public record Pagination
{
    /// <summary>
    /// Number of elements to return per page. Default is 1000.
    /// </summary>
    public long Size { get; set; } = 1000;

    /// <summary>
    /// 0-based page index for pagination.
    /// </summary>
    public long Page { get; set; } = 0;
}
