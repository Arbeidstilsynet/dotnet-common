using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Response;
using Arbeidstilsynet.Common.Enhetsregisteret.Models;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Ports;

/// <summary>
/// Interface describing common Enhetsregisteret operations.
/// </summary>
/// <remarks>
/// This interface is not registered by <c>AddEnhetsregisteret(...)</c>. It is kept as a starting point
/// for consumers who want to build their own adapter over the generated <c>EnhetsregisteretClient</c>.
/// </remarks>
public interface IEnhetsregisteret
{
    /// <summary>
    /// Get the <see cref="Underenhet"/> with <see cref="organisasjonsnummer"/>.
    /// </summary>
    /// <param name="organisasjonsnummer"></param>
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
    Task<PaginationResult<Enhet>?> SearchEnheter(
        SearchEnheterQuery searchParameters,
        Pagination pagination
    );

    /// <summary>
    /// Hent oppdateringshistorikk for underenheter i enhetsregisteret.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<PaginationResult<OppdateringerUnderenhet>?> GetOppdateringerUnderenheter(
        GetOppdateringerQuery query,
        Pagination pagination
    );

    /// <summary>
    /// Hent oppdateringshistorikk for enheter i enhetsregisteret.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<PaginationResult<OppdateringerEnhet>?> GetOppdateringerEnheter(
        GetOppdateringerQuery query,
        Pagination pagination
    );
}
