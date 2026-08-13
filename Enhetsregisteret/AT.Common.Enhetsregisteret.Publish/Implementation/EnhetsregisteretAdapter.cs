using Arbeidstilsynet.Common.Enhetsregisteret;
using Arbeidstilsynet.Common.Enhetsregisteret.Models;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using Microsoft.Kiota.Abstractions;
using EnheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Enheter.EnheterRequestBuilder.EnheterRequestBuilderGetQueryParameters;
using OppdateringerEnheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Oppdateringer.Enheter.EnheterRequestBuilder.EnheterRequestBuilderGetQueryParameters;
using OppdateringerUnderenheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Oppdateringer.Underenheter.UnderenheterRequestBuilder.UnderenheterRequestBuilderGetQueryParameters;
using UnderenheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Underenheter.UnderenheterRequestBuilder.UnderenheterRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Implementation;

/// <summary>
/// Thin adapter over the generated <see cref="EnhetsregisteretClient"/> that implements
/// <see cref="IEnhetsregisteret"/> using the generated models directly.
/// </summary>
internal sealed class EnhetsregisteretAdapter : IEnhetsregisteret
{
    private const int NotFoundStatusCode = 404;

    private readonly EnhetsregisteretClient _client;

    public EnhetsregisteretAdapter(EnhetsregisteretClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Henter en <see cref="Enhet"/> på organisasjonsnummer. Returnerer <c>null</c> dersom
    /// Enhetsregisteret svarer med HTTP 404 (ukjent organisasjonsnummer), slik at kallere kan
    /// håndtere "ikke funnet" som et tomt resultat i stedet for et unntak.
    /// </summary>
    public Task<Enhet?> GetEnhet(
        string organisasjonsnummer,
        CancellationToken cancellationToken = default
    ) =>
        NullOnNotFound(async () =>
        {
            var response = await _client
                .Enhetsregisteret.Api.Enheter[organisasjonsnummer]
                .GetAsync(cancellationToken: cancellationToken);

            ThrowIfSlettet(response?.SlettetEnhet);
            return response?.Enhet;
        });

    /// <summary>
    /// Henter en <see cref="Underenhet"/> på organisasjonsnummer. Returnerer <c>null</c> dersom
    /// Enhetsregisteret svarer med HTTP 404 (ukjent organisasjonsnummer), slik at kallere kan
    /// håndtere "ikke funnet" som et tomt resultat i stedet for et unntak.
    /// </summary>
    public Task<Underenhet?> GetUnderenhet(
        string organisasjonsnummer,
        CancellationToken cancellationToken = default
    ) =>
        NullOnNotFound(async () =>
        {
            var response = await _client
                .Enhetsregisteret.Api.Underenheter[organisasjonsnummer]
                .GetAsync(cancellationToken: cancellationToken);

            ThrowIfSlettet(response?.SlettetUnderEnhet);
            return response?.Underenhet;
        });

    public Task<Enheter?> SearchEnheter(
        Action<EnheterQueryParameters>? configureQuery = null,
        CancellationToken cancellationToken = default
    ) =>
        _client.Enhetsregisteret.Api.Enheter.GetAsync(Configure(configureQuery), cancellationToken);

    public Task<Underenheter?> SearchUnderenheter(
        Action<UnderenheterQueryParameters>? configureQuery = null,
        CancellationToken cancellationToken = default
    ) =>
        _client.Enhetsregisteret.Api.Underenheter.GetAsync(
            Configure(configureQuery),
            cancellationToken
        );

    public Task<OppdateringerEnheter?> GetOppdateringerEnheter(
        Action<OppdateringerEnheterQueryParameters>? configureQuery = null,
        CancellationToken cancellationToken = default
    ) =>
        _client.Enhetsregisteret.Api.Oppdateringer.Enheter.GetAsync(
            Configure(configureQuery),
            cancellationToken
        );

    public Task<OppdateringerUnderenheter?> GetOppdateringerUnderenheter(
        Action<OppdateringerUnderenheterQueryParameters>? configureQuery = null,
        CancellationToken cancellationToken = default
    ) =>
        _client.Enhetsregisteret.Api.Oppdateringer.Underenheter.GetAsync(
            Configure(configureQuery),
            cancellationToken
        );

    private static Action<RequestConfiguration<TQueryParameters>>? Configure<TQueryParameters>(
        Action<TQueryParameters>? configureQuery
    )
        where TQueryParameters : class, new() =>
        configureQuery is null ? null : config => configureQuery(config.QueryParameters);

    private static void ThrowIfSlettet(SlettetEnhet? slettetEnhet)
    {
        if (slettetEnhet is null)
        {
            return;
        }

        throw new VirksomhetSlettetException(
            slettetEnhet.Organisasjonsnummer,
            slettetEnhet.Navn,
            slettetEnhet.Slettedato
        );
    }

    private static void ThrowIfSlettet(SlettetUnderEnhet? slettetUnderEnhet)
    {
        if (slettetUnderEnhet is null)
        {
            return;
        }

        throw new VirksomhetSlettetException(
            slettetUnderEnhet.Organisasjonsnummer,
            slettetUnderEnhet.Navn,
            slettetUnderEnhet.Slettedato
        );
    }

    private static async Task<T?> NullOnNotFound<T>(Func<Task<T?>> fetch)
        where T : class
    {
        try
        {
            return await fetch();
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == NotFoundStatusCode)
        {
            return null;
        }
    }
}
