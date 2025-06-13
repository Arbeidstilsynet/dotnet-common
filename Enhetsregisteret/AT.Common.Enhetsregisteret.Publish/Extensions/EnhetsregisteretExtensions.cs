using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Response;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Extensions;

/// <summary>
/// Utvidelser for å forenkle APIet mot enhetsregisteret.
/// </summary>
public static class EnhetsregisteretExtensions
{
    /// <summary>
    /// Hent alle underenheter basert på organisasjonsnummeret til overordnet enhet.
    /// </summary>
    /// <param name="enhetsregisteret"></param>
    /// <param name="organisasjonsnummerForOverordnetEnhet">Organisasjonsnummeret til overordnet enhet.</param>
    /// <param name="antallUnderenheter">Antall underenheter som skal hentes. Standard er 1000.</param>
    /// <returns>Alle <see cref="Underenhet"/> som er underordnet hovedenheten med det gitte orgnummeret.</returns>
    public static async Task<IEnumerable<Underenhet>> GetUnderenheter(this IEnhetsregisteret enhetsregisteret,
        string organisasjonsnummerForOverordnetEnhet, int antallUnderenheter = 1000)
    {
        organisasjonsnummerForOverordnetEnhet.ValidateOrgnummerOrThrow(nameof(organisasjonsnummerForOverordnetEnhet));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(antallUnderenheter, 0);
        
        var query = new SearchEnheterQuery
        {
            OverordnetEnhetOrganisasjonsnummer = organisasjonsnummerForOverordnetEnhet
        };

        var pagination = new Pagination
        {
            Size = antallUnderenheter
        };
        
        var paginatedResult = await enhetsregisteret.SearchUnderenheter(query, pagination);
        
        return paginatedResult?.Elements ?? [];
    }

    /// <summary>
    /// Hent opp til flere underenheter basert på en liste med organisasjonsnumre.
    /// </summary>
    /// <param name="organisasjonsnumre">Organisasjonsnumrene til underenhetene.</param>
    /// <param name="antallUnderenheter">Antall underenheter som skal hentes. Standard er 1000.</param>
    /// <returns><see cref="Underenhet"/> som matcher orgnumre.</returns>
    public static async Task<IEnumerable<Underenhet>> GetUnderenheter(
        this IEnhetsregisteret enhetsregisteret,
        IEnumerable<string> organisasjonsnumre,
        int antallUnderenheter = 1000
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(antallUnderenheter, 0);

        var validOrganisasjonsnummer = organisasjonsnumre.Where(orgnummer => orgnummer.IsValidOrgnummer()).ToArray();

        if (validOrganisasjonsnummer.Length == 0)
        {
            return [];
        }
        
        var query = new SearchEnheterQuery
        {
            Organisasjonsnummer = validOrganisasjonsnummer
        };

        var pagination = new Pagination
        {
            Size = antallUnderenheter
        };

        var paginatedResult = await enhetsregisteret.SearchUnderenheter(query, pagination);
        
        return paginatedResult?.Elements ?? [];
    }

    /// <summary>
    /// Hent opp til flere enheter basert på en liste med organisasjonsnumre.
    /// </summary>
    /// <param name="organisasjonsnumre">Organisasjonsnumrene til enhetene.</param>
    /// <param name="antallEnheter">Antall enheter som skal hentes. Standard er 1000.</param>
    /// <returns><see cref="Enhet"/> som matcher orgnumrene.</returns>
    public static async Task<IEnumerable<Enhet>> GetEnheter(this IEnhetsregisteret enhetsregisteret, IEnumerable<string> organisasjonsnumre, int antallEnheter = 1000)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(antallEnheter, 0);

        var validOrganisasjonsnummer = organisasjonsnumre.Where(orgnummer => orgnummer.IsValidOrgnummer()).ToArray();

        if (validOrganisasjonsnummer.Length == 0)
        {
            return [];
        }

        var query = new SearchEnheterQuery
        {
            Organisasjonsnummer = validOrganisasjonsnummer
        };

        var pagination = new Pagination
        {
            Size = antallEnheter
        };

        var paginatedResult = await enhetsregisteret.SearchEnheter(query, pagination);
        
        return paginatedResult?.Elements ?? [];
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