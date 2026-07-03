namespace Arbeidstilsynet.Shared.Extensions;

/// <summary>
/// Internal abstraction over an arbitrary paginated response, so that pagination logic can be
/// implemented once, independent of the concrete response shape.
/// </summary>
/// <typeparam name="T">The type of the elements on a page.</typeparam>
internal interface IPaginatedResponse<out T>
{
    /// <summary>
    /// Elements on the current page.
    /// </summary>
    IReadOnlyList<T> Elements { get; }

    /// <summary>
    /// Total number of pages available for the query.
    /// </summary>
    long TotalPages { get; }
}

internal sealed record PaginatedResponse<T>(IReadOnlyList<T> Elements, long TotalPages)
    : IPaginatedResponse<T>;

internal static class PaginationExtensions
{
    /// <summary>
    /// Enumerates all elements across all pages, following pagination by repeatedly invoking
    /// <paramref name="fetchPage"/> with a zero-based page index until the last page is reached.
    /// </summary>
    /// <typeparam name="T">The type of the elements on a page.</typeparam>
    /// <param name="fetchPage">
    /// Fetches a page given its zero-based index. Returning <see langword="null"/> stops enumeration.
    /// </param>
    public static async IAsyncEnumerable<T> EnumeratePaginatedElements<T>(
        Func<int, Task<IPaginatedResponse<T>?>> fetchPage
    )
    {
        const int firstPage = 0;

        var result = await fetchPage(firstPage);

        if (result == null)
        {
            yield break;
        }

        foreach (var element in result.Elements)
        {
            yield return element;
        }

        var lastPage = result.TotalPages - 1;
        for (var page = firstPage + 1; page <= lastPage; page++)
        {
            result = await fetchPage(page);

            if (result == null)
            {
                yield break;
            }

            foreach (var element in result.Elements)
            {
                yield return element;
            }
        }
    }
}
