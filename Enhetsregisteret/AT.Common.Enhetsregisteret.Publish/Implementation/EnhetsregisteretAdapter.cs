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
    private readonly EnhetsregisteretClient _client;

    public EnhetsregisteretAdapter(EnhetsregisteretClient client)
    {
        _client = client;
    }

    public async Task<Enhet?> GetEnhet(
        string organisasjonsnummer,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _client
            .Enhetsregisteret.Api.Enheter[organisasjonsnummer]
            .GetAsync(cancellationToken: cancellationToken);

        return response?.Enhet;
    }

    public async Task<Underenhet?> GetUnderenhet(
        string organisasjonsnummer,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _client
            .Enhetsregisteret.Api.Underenheter[organisasjonsnummer]
            .GetAsync(cancellationToken: cancellationToken);

        return response?.Underenhet;
    }

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
}
