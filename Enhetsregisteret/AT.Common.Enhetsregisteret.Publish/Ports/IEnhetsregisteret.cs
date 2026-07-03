using Arbeidstilsynet.Common.Enhetsregisteret.Models;
using EnheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Enheter.EnheterRequestBuilder.EnheterRequestBuilderGetQueryParameters;
using OppdateringerEnheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Oppdateringer.Enheter.EnheterRequestBuilder.EnheterRequestBuilderGetQueryParameters;
using OppdateringerUnderenheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Oppdateringer.Underenheter.UnderenheterRequestBuilder.UnderenheterRequestBuilderGetQueryParameters;
using UnderenheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Underenheter.UnderenheterRequestBuilder.UnderenheterRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Ports;

/// <summary>
/// Interface describing common Enhetsregisteret operations, exposing the generated models directly.
/// </summary>
/// <remarks>
/// An implementation is registered by <c>AddEnhetsregisteret(...)</c>. It is a thin adapter over the
/// generated <c>EnhetsregisteretClient</c> and can be used directly or extended via
/// <see cref="Extensions.EnhetsregisteretExtensions"/>.
/// </remarks>
public interface IEnhetsregisteret
{
    /// <summary>
    /// Hent en <see cref="Underenhet"/> basert på organisasjonsnummeret.
    /// </summary>
    /// <param name="organisasjonsnummer">Organisasjonsnummeret til underenheten.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// En <see cref="Underenhet"/>, eller <c>null</c> dersom underenheten ikke finnes
    /// (Enhetsregisteret svarer med HTTP 404).
    /// </returns>
    /// <remarks>
    /// Kun HTTP 404 fanges og oversettes til <c>null</c>. Andre feil (f.eks. 400 for ugyldig
    /// organisasjonsnummer eller 5xx) kastes videre som unntak.
    /// </remarks>
    Task<Underenhet?> GetUnderenhet(
        string organisasjonsnummer,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Hent en <see cref="Enhet"/> basert på organisasjonsnummeret.
    /// </summary>
    /// <param name="organisasjonsnummer">Organisasjonsnummeret til enheten.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// En <see cref="Enhet"/>, eller <c>null</c> dersom enheten ikke finnes
    /// (Enhetsregisteret svarer med HTTP 404).
    /// </returns>
    /// <remarks>
    /// Kun HTTP 404 fanges og oversettes til <c>null</c>. Andre feil (f.eks. 400 for ugyldig
    /// organisasjonsnummer eller 5xx) kastes videre som unntak.
    /// </remarks>
    Task<Enhet?> GetEnhet(
        string organisasjonsnummer,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Søk etter underenheter basert på søkeparametere.
    /// </summary>
    /// <param name="configureQuery">Konfigurer søkeparametrene, inkludert paginering.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Underenhetene som matcher søket</returns>
    Task<Underenheter?> SearchUnderenheter(
        Action<UnderenheterQueryParameters>? configureQuery = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Søk etter enheter basert på søkeparametere.
    /// </summary>
    /// <param name="configureQuery">Konfigurer søkeparametrene, inkludert paginering.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Enhetene som matcher søket</returns>
    Task<Enheter?> SearchEnheter(
        Action<EnheterQueryParameters>? configureQuery = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Hent oppdateringshistorikk for underenheter i enhetsregisteret.
    /// </summary>
    /// <param name="configureQuery">Konfigurer spørringen, inkludert paginering.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<OppdateringerUnderenheter?> GetOppdateringerUnderenheter(
        Action<OppdateringerUnderenheterQueryParameters>? configureQuery = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Hent oppdateringshistorikk for enheter i enhetsregisteret.
    /// </summary>
    /// <param name="configureQuery">Konfigurer spørringen, inkludert paginering.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<OppdateringerEnheter?> GetOppdateringerEnheter(
        Action<OppdateringerEnheterQueryParameters>? configureQuery = null,
        CancellationToken cancellationToken = default
    );
}
