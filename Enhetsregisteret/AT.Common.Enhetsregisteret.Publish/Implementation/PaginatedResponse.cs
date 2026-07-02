using Arbeidstilsynet.Common.Enhetsregisteret.Models;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Implementation;

/// <summary>
/// Internal abstraction over the various generated paginated response wrappers
/// (<see cref="Enheter"/>, <see cref="Underenheter"/>, <see cref="OppdateringerEnheter"/>,
/// <see cref="OppdateringerUnderenheter"/>) so that pagination logic can be implemented once,
/// independent of the concrete response shape.
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

internal static class PaginatedResponseExtensions
{
    public static IPaginatedResponse<Enhet> ToPaginatedResponse(this Enheter response) =>
        Create(response.Embedded?.Enheter, response.Page);

    public static IPaginatedResponse<Underenhet> ToPaginatedResponse(this Underenheter response) =>
        Create(response.Embedded?.Underenheter, response.Page);

    public static IPaginatedResponse<OppdateringerEnhet> ToPaginatedResponse(
        this OppdateringerEnheter response
    ) => Create(response.Embedded?.OppdaterteEnheter, response.Page);

    public static IPaginatedResponse<OppdateringerUnderenhet> ToPaginatedResponse(
        this OppdateringerUnderenheter response
    ) => Create(response.Embedded?.OppdaterteUnderenheter, response.Page);

    private static IPaginatedResponse<T> Create<T>(IReadOnlyList<T>? elements, Page? page) =>
        new PaginatedResponse<T>(elements ?? [], ComputeTotalPages(page));

    private static long ComputeTotalPages(Page? page)
    {
        var pageSize = (long)(page?.Size ?? 0);

        if (pageSize == 0)
        {
            return 0;
        }

        var totalElements = (long)(page?.TotalElements ?? 0);
        var partialPage = totalElements % pageSize == 0 ? 0 : 1;

        return totalElements / pageSize + partialPage;
    }
}
