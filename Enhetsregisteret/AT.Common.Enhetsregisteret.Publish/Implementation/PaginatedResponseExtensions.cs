using Arbeidstilsynet.Common.Enhetsregisteret.Models;
using Arbeidstilsynet.Shared.Extensions;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Implementation;

/// <summary>
/// Maps the various generated paginated response wrappers
/// (<see cref="Enheter"/>, <see cref="Underenheter"/>, <see cref="OppdateringerEnheter"/>,
/// <see cref="OppdateringerUnderenheter"/>) onto the shared <see cref="IPaginatedResponse{T}"/>
/// abstraction so that pagination logic can be implemented once, independent of the concrete
/// response shape.
/// </summary>
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
        if (page is null)
        {
            return 0;
        }

        var pageSize = (long)(page.Size ?? 0);

        if (pageSize == 0)
        {
            return 0;
        }

        var totalElements = (long)(page.TotalElements ?? 0);
        var partialPage = totalElements % pageSize == 0 ? 0 : 1;

        return totalElements / pageSize + partialPage;
    }
}
