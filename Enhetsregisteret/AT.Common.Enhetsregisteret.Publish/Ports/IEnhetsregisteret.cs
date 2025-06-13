using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Response;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Ports;

/// <summary>
/// Interface which can be dependency injected to use methods of Enhetsregisteret
/// </summary>
public interface IEnhetsregisteret
{
    /// <summary>
    /// Hent en <see cref="Underenhet"/> basert på organisasjonsnummeret.
    /// </summary>
    /// <param name="organisasjonsnummer">Organisasjonsnummeret til underenheten</param>
    /// <returns>En <see cref="Underenhet"/>. Null hvis enheten ikke finnes, eller hvis det oppstår en feil under henting.</returns>
    Task<Underenhet?> GetUnderenhet(string organisasjonsnummer);

    /// <summary>
    /// Hent en <see cref="Enhet"/> basert på organisasjonsnummeret.
    /// </summary>
    /// <param name="organisasjonsnummer">Organisasjonsnummeret til enheten.</param>
    /// <returns>En <see cref="Enhet"/>. Null hvis enheten ikke finnes, eller hvis det oppstår en feil under henting.</returns>
    Task<Enhet?> GetEnhet(string organisasjonsnummer);
    
    /// <summary>
    /// Søk etter underenheter basert på søkeparametere.
    /// </summary>
    /// <param name="searchParameters">Søkeparametrene</param>
    /// <param name="pagination"></param>
    /// <returns>Underenhetene som matcher søket</returns>
    Task<PaginationResult<Underenhet>?> SearchUnderenheter(
        SearchEnheterQuery searchParameters,
        Pagination pagination
    );

    /// <summary>
    /// Søk etter enheter basert på søkeparametere.
    /// </summary>
    /// <param name="searchParameters">Søkeparametrene</param>
    /// <param name="pagination"></param>
    /// <returns>Enhetene som matcher søket</returns>
    Task<PaginationResult<Enhet>?> SearchEnheter(SearchEnheterQuery searchParameters, Pagination pagination);

    /// <summary>
    /// Hent oppdateringshistorikk for underenheter i enhetsregisteret.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<PaginationResult<Oppdatering>?> GetOppdateringerUnderenheter(
        GetOppdateringerQuery query,
        Pagination pagination
    );

    /// <summary>
    /// Hent oppdateringshistorikk for enheter i enhetsregisteret.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<PaginationResult<Oppdatering>?> GetOppdateringerEnheter(GetOppdateringerQuery query, Pagination pagination);
}
