using System.Globalization;
using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Models;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using Arbeidstilsynet.Common.Enhetsregisteret.Validation.Extensions;
using EnheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Enheter.EnheterRequestBuilder.EnheterRequestBuilderGetQueryParameters;
using OppdateringerEnheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Oppdateringer.Enheter.EnheterRequestBuilder.EnheterRequestBuilderGetQueryParameters;
using OppdateringerUnderenheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Oppdateringer.Underenheter.UnderenheterRequestBuilder.UnderenheterRequestBuilderGetQueryParameters;
using UnderenheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Underenheter.UnderenheterRequestBuilder.UnderenheterRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Extensions;

/// <summary>
/// Extensions for simplifying common access patterns to Enhetsregisteret.
/// </summary>
public static class EnhetsregisteretExtensions
{
    private const int PageSize = 1000;

    /// <summary>
    /// Gets <see cref="Underenhet"/>s that are under the given hovedenhet.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="organisasjonsnummerForOverordnetEnhet">Identifies the hovedenhet</param>
    /// <returns></returns>
    public static Task<IEnumerable<Underenhet>> GetUnderenheterByHovedenhet(
        this IEnhetsregisteret enhetsregisteret,
        string organisasjonsnummerForOverordnetEnhet
    )
    {
        organisasjonsnummerForOverordnetEnhet.ValidateOrgnummerOrThrow(
            nameof(organisasjonsnummerForOverordnetEnhet)
        );

        return enhetsregisteret
            .EnumerateUnderenheter(q => q.OverordnetEnhet = organisasjonsnummerForOverordnetEnhet)
            .ToListAsync();
    }

    /// <summary>
    /// Gets <see cref="Underenhet"/>s based on a list of organizational numbers.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="organisasjonsnumre"></param>
    /// <returns><see cref="Underenhet"/> matching <paramref name="organisasjonsnumre"/>.</returns>
    public static Task<IEnumerable<Underenhet>> GetUnderenheter(
        this IEnhetsregisteret enhetsregisteret,
        IEnumerable<string> organisasjonsnumre
    )
    {
        var validOrganisasjonsnummer = organisasjonsnumre
            .Where(orgnummer => orgnummer.IsValidOrgnummer())
            .ToArray();

        if (validOrganisasjonsnummer.Length == 0)
        {
            return Task.FromResult<IEnumerable<Underenhet>>([]);
        }

        return enhetsregisteret
            .EnumerateUnderenheter(q => q.Organisasjonsnummer = validOrganisasjonsnummer)
            .ToListAsync();
    }

    /// <summary>
    /// Gets <see cref="Enhet"/>s based on the organizational number.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="organisasjonsnumre"></param>
    /// <returns><see cref="Enhet"/>s matching <paramref name="organisasjonsnumre"/></returns>
    public static Task<IEnumerable<Enhet>> GetEnheter(
        this IEnhetsregisteret enhetsregisteret,
        IEnumerable<string> organisasjonsnumre
    )
    {
        var validOrganisasjonsnummer = organisasjonsnumre
            .Where(orgnummer => orgnummer.IsValidOrgnummer())
            .ToArray();

        if (validOrganisasjonsnummer.Length == 0)
        {
            return Task.FromResult<IEnumerable<Enhet>>([]);
        }

        return enhetsregisteret
            .EnumerateEnheter(q => q.Organisasjonsnummer = validOrganisasjonsnummer)
            .ToListAsync();
    }

    /// <summary>
    /// Enumerates all <see cref="Underenhet"/>s matching the configured search query, following pagination.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="configureQuery">Configures the search query parameters (paging is applied automatically).</param>
    /// <returns></returns>
    public static IAsyncEnumerable<Underenhet> EnumerateUnderenheter(
        this IEnhetsregisteret enhetsregisteret,
        Action<UnderenheterQueryParameters> configureQuery
    )
    {
        return EnumeratePaginatedElements(async page =>
            (
                await enhetsregisteret.SearchUnderenheter(q =>
                {
                    configureQuery(q);
                    q.Page = page;
                    q.Size = PageSize;
                })
            )?.ToPaginatedResponse()
        );
    }

    /// <summary>
    /// Enumerates all <see cref="Enhet"/>s matching the configured search query, following pagination.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="configureQuery">Configures the search query parameters (paging is applied automatically).</param>
    /// <returns></returns>
    public static IAsyncEnumerable<Enhet> EnumerateEnheter(
        this IEnhetsregisteret enhetsregisteret,
        Action<EnheterQueryParameters> configureQuery
    )
    {
        return EnumeratePaginatedElements(async page =>
            (
                await enhetsregisteret.SearchEnheter(q =>
                {
                    configureQuery(q);
                    q.Page = page;
                    q.Size = PageSize;
                })
            )?.ToPaginatedResponse()
        );
    }

    /// <summary>
    /// Enumerates all <see cref="OppdateringerUnderenhet"/> matching the configured query, following pagination.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="configureQuery">Configures the query parameters (paging is applied automatically).</param>
    /// <returns></returns>
    public static IAsyncEnumerable<OppdateringerUnderenhet> EnumerateOppdateringerUnderenheter(
        this IEnhetsregisteret enhetsregisteret,
        Action<OppdateringerUnderenheterQueryParameters> configureQuery
    )
    {
        return EnumeratePaginatedElements(async page =>
            (
                await enhetsregisteret.GetOppdateringerUnderenheter(q =>
                {
                    configureQuery(q);
                    q.Page = page.ToString(CultureInfo.InvariantCulture);
                    q.Size = PageSize.ToString(CultureInfo.InvariantCulture);
                })
            )?.ToPaginatedResponse()
        );
    }

    /// <summary>
    /// Enumerates all <see cref="OppdateringerEnhet"/> matching the configured query, following pagination.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="configureQuery">Configures the query parameters (paging is applied automatically).</param>
    /// <returns></returns>
    public static IAsyncEnumerable<OppdateringerEnhet> EnumerateOppdateringerEnheter(
        this IEnhetsregisteret enhetsregisteret,
        Action<OppdateringerEnheterQueryParameters> configureQuery
    )
    {
        return EnumeratePaginatedElements(async page =>
            (
                await enhetsregisteret.GetOppdateringerEnheter(q =>
                {
                    configureQuery(q);
                    q.Page = page.ToString(CultureInfo.InvariantCulture);
                    q.Size = PageSize.ToString(CultureInfo.InvariantCulture);
                })
            )?.ToPaginatedResponse()
        );
    }

    internal static async IAsyncEnumerable<T> EnumeratePaginatedElements<T>(
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

    private static async Task<IEnumerable<T>> ToListAsync<T>(
        this IAsyncEnumerable<T> asyncEnumerable
    )
    {
        var list = new List<T>();

        await foreach (var item in asyncEnumerable)
        {
            list.Add(item);
        }

        return list;
    }
}
