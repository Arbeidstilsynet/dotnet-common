using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Response;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Extensions;

/// <summary>
/// Extensions for simplifying common access patterns to Enhetsregisteret.
/// </summary>
public static class EnhetsregisteretExtensions
{
    /// <summary>
    /// Gets <see cref="Underenhet"/>s that are under the given hovedenhet.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="organisasjonsnummerForOverordnetEnhet">Identifies the hovedenhet</param>
    /// <returns></returns>
    public static Task<IEnumerable<Underenhet>> GetUnderenheterByHovedenhet(this IEnhetsregisteret enhetsregisteret,
        string organisasjonsnummerForOverordnetEnhet)
    {
        organisasjonsnummerForOverordnetEnhet.ValidateOrgnummerOrThrow(nameof(organisasjonsnummerForOverordnetEnhet));
        
        var query = new SearchEnheterQuery
        {
            OverordnetEnhetOrganisasjonsnummer = organisasjonsnummerForOverordnetEnhet
        };

        return EnumeratePaginatedElements(pagination => enhetsregisteret.SearchUnderenheter(query, pagination))
            .ToListAsync();
    }

    /// <summary>
    /// Gets <see cref="Underenhet"/>s based on a list of organizational numbers.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="organisasjonsnumre"></param>
    /// <returns><see cref="Underenhet"/> matching <see cref="organisasjonsnumre"/>.</returns>
    public static Task<IEnumerable<Underenhet>> GetUnderenheter(
        this IEnhetsregisteret enhetsregisteret,
        IEnumerable<string> organisasjonsnumre
    )
    {
        var validOrganisasjonsnummer = organisasjonsnumre.Where(orgnummer => orgnummer.IsValidOrgnummer()).ToArray();

        if (validOrganisasjonsnummer.Length == 0)
        {
            return Task.FromResult<IEnumerable<Underenhet>>([]);
        }
        
        var query = new SearchEnheterQuery
        {
            Organisasjonsnummer = validOrganisasjonsnummer
        };

        return EnumeratePaginatedElements(pagination => enhetsregisteret.SearchUnderenheter(query, pagination))
            .ToListAsync();
    }

    /// <summary>
    /// Gets <see cref="Enhet"/>s based on the organizational number.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="organisasjonsnumre"></param>
    /// <returns><see cref="Enhet"/>s matching <see cref="organisasjonsnumre"/></returns>
    public static Task<IEnumerable<Enhet>> GetEnheter(this IEnhetsregisteret enhetsregisteret, IEnumerable<string> organisasjonsnumre)
    {
        var validOrganisasjonsnummer = organisasjonsnumre.Where(orgnummer => orgnummer.IsValidOrgnummer()).ToArray();

        if (validOrganisasjonsnummer.Length == 0)
        {
            return Task.FromResult<IEnumerable<Enhet>>([]);
        }

        var query = new SearchEnheterQuery
        {
            Organisasjonsnummer = validOrganisasjonsnummer
        };

        return EnumeratePaginatedElements(pagination => enhetsregisteret.SearchEnheter(query, pagination))
            .ToListAsync();
    }
    
    /// <summary>
    /// Henter alle underenheter basert på queryen.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public static IAsyncEnumerable<Underenhet> SearchUnderenheter(
        this IEnhetsregisteret enhetsregisteret,
        SearchEnheterQuery query
    )
    {
        return EnumeratePaginatedElements(pagination => enhetsregisteret.SearchUnderenheter(query, pagination));
    }

    /// <summary>
    /// Henter alle enheter basert på queryen.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public static IAsyncEnumerable<Enhet> SearchEnheter(
        this IEnhetsregisteret enhetsregisteret,
        SearchEnheterQuery query
    )
    {
        return EnumeratePaginatedElements(pagination => enhetsregisteret.SearchEnheter(query, pagination));
    }

    /// <summary>
    /// Henter alle oppdateringer på underenheter basert på queryen.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public static IAsyncEnumerable<Oppdatering> GetOppdateringerUnderenheter(
        this IEnhetsregisteret enhetsregisteret,
        GetOppdateringerQuery query
    )
    {
        return EnumeratePaginatedElements(pagination =>
            enhetsregisteret.GetOppdateringerUnderenheter(query, pagination)
        );
    }

    /// <summary>
    /// Henter alle oppdateringer på enheter basert på queryen.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public static IAsyncEnumerable<Oppdatering> GetOppdateringerEnheter(
        this IEnhetsregisteret enhetsregisteret,
        GetOppdateringerQuery query
    )
    {
        return EnumeratePaginatedElements(pagination => enhetsregisteret.GetOppdateringerEnheter(query, pagination));
    }

    internal static async IAsyncEnumerable<T> EnumeratePaginatedElements<T>(
        Func<Pagination, Task<PaginationResult<T>?>> fetchFunction
    )
    {
        const int FIRST_PAGE = 0;

        var pagination = new Pagination { Page = FIRST_PAGE, Size = 1000 };

        var result = await fetchFunction(pagination);

        if (result == null)
        {
            yield break;
        }

        foreach (var element in result.Elements)
        {
            yield return element;
        }

        var lastPage = result.TotalPages() - 1;
        for (var nextPage = FIRST_PAGE + 1; nextPage <= lastPage; nextPage++)
        {
            result = await fetchFunction(pagination with { Page = nextPage });

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
    
    private static async Task<IEnumerable<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
    {
        var list = new List<T>();
        
        await foreach(var item in asyncEnumerable)
        {
            list.Add(item);
        }
        
        return list;
    }

    private static long TotalPages<T>(this PaginationResult<T> paginationResult)
    {
        if (paginationResult.PageSize == 0)
        {
            return 0;
        }

        var partialPage = paginationResult.TotalElements % paginationResult.PageSize == 0 ? 0 : 1;

        var totalFullPages = paginationResult.TotalElements / paginationResult.PageSize;

        return totalFullPages + partialPage;
    }
}