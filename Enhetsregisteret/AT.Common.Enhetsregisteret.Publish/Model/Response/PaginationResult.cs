namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Response;

/// <summary>
/// Resultat av paginert spørring.
/// </summary>
/// <typeparam name="T"></typeparam>
public record PaginationResult<T>
{
    /// <summary>
    /// Total antall elementer i alle sidene.
    /// </summary>
    public long TotalElements { get; init; }

    /// <summary>
    /// Antall elementer per side.
    /// </summary>
    public long PageSize { get; init; }

    /// <summary>
    /// 0-basert sideindeks.
    /// </summary>
    public long PageIndex { get; init; }

    /// <summary>
    /// Elementene på denne siden.
    /// </summary>
    public IEnumerable<T> Elements { get; init; } = [];
}