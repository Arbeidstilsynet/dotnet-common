using System.Globalization;
using Arbeidstilsynet.Common.Enhetsregisteret.Models;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using Arbeidstilsynet.Common.Enhetsregisteret.Validation.Extensions;
using Arbeidstilsynet.Shared.Implementation;
using EnheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Enheter.EnheterRequestBuilder.EnheterRequestBuilderGetQueryParameters;
using OppdateringerEnheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Oppdateringer.Enheter.EnheterRequestBuilder.EnheterRequestBuilderGetQueryParameters;
using OppdateringerUnderenheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Oppdateringer.Underenheter.UnderenheterRequestBuilder.UnderenheterRequestBuilderGetQueryParameters;
using UnderenheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Underenheter.UnderenheterRequestBuilder.UnderenheterRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Implementation;

internal sealed class EnhetsregisteretStub : IEnhetsregisteret
{
    private const string EnhetNumber = "974761211";
    private const string UnderenhetNumber = "974761076";
    private const string Name = "Eksempel AS";

    public Task<Enhet?> GetEnhet(
        string organisasjonsnummer,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        organisasjonsnummer.ValidateOrgnummerOrThrow(nameof(organisasjonsnummer));
        return Task.FromResult(organisasjonsnummer == EnhetNumber ? NewEnhet() : null);
    }

    public Task<Underenhet?> GetUnderenhet(
        string organisasjonsnummer,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        organisasjonsnummer.ValidateOrgnummerOrThrow(nameof(organisasjonsnummer));
        return Task.FromResult(organisasjonsnummer == UnderenhetNumber ? NewUnderenhet() : null);
    }

    public Task<Enheter?> SearchEnheter(
        Action<EnheterQueryParameters>? configureQuery = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = Query(
            configureQuery,
            cancellationToken,
            "Organisasjonsnummer",
            "Navn",
            "Page",
            "Size"
        );
        StubQuery.Page(query.Page, query.Size);
        var matches = Matches(query.Organisasjonsnummer, query.Navn, EnhetNumber);
        return Task.FromResult<Enheter?>(
            new()
            {
                Embedded = new()
                {
                    Enheter = matches && (query.Page ?? 0) == 0 ? [NewEnhet()] : [],
                },
                Page = NewPage(query.Page, query.Size, matches),
            }
        );
    }

    public Task<Underenheter?> SearchUnderenheter(
        Action<UnderenheterQueryParameters>? configureQuery = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = Query(
            configureQuery,
            cancellationToken,
            "Organisasjonsnummer",
            "Navn",
            "OverordnetEnhet",
            "Page",
            "Size"
        );
        StubQuery.Page(query.Page, query.Size);
        var matches =
            Matches(query.Organisasjonsnummer, query.Navn, UnderenhetNumber)
            && (query.OverordnetEnhet is null or EnhetNumber);
        return Task.FromResult<Underenheter?>(
            new()
            {
                Embedded = new()
                {
                    Underenheter = matches && (query.Page ?? 0) == 0 ? [NewUnderenhet()] : [],
                },
                Page = NewPage(query.Page, query.Size, matches),
            }
        );
    }

    public Task<OppdateringerEnheter?> GetOppdateringerEnheter(
        Action<OppdateringerEnheterQueryParameters>? configureQuery = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = Query(
            configureQuery,
            cancellationToken,
            "Page",
            "Size",
            "Dato",
            "Oppdateringsid"
        );
        var page = ParsePageValue(query.Page);
        var size = ParsePageValue(query.Size);
        StubQuery.Page(page, size);
        return Task.FromResult<OppdateringerEnheter?>(
            new()
            {
                Embedded = new() { OppdaterteEnheter = [] },
                Page = NewPage(page, size, false),
            }
        );
    }

    public Task<OppdateringerUnderenheter?> GetOppdateringerUnderenheter(
        Action<OppdateringerUnderenheterQueryParameters>? configureQuery = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = Query(
            configureQuery,
            cancellationToken,
            "Page",
            "Size",
            "Dato",
            "Oppdateringsid"
        );
        var page = ParsePageValue(query.Page);
        var size = ParsePageValue(query.Size);
        StubQuery.Page(page, size);
        return Task.FromResult<OppdateringerUnderenheter?>(
            new()
            {
                Embedded = new() { OppdaterteUnderenheter = [] },
                Page = NewPage(page, size, false),
            }
        );
    }

    private static T Query<T>(
        Action<T>? configure,
        CancellationToken cancellationToken,
        params string[] supported
    )
        where T : new()
    {
        cancellationToken.ThrowIfCancellationRequested();
        var query = new T();
        configure?.Invoke(query);
        StubQuery.Validate(query, supported);
        return query;
    }

    private static bool Matches(string[]? numbers, string? name, string number) =>
        (numbers is null || numbers.Contains(number))
        && (name is null || Name.Contains(name, StringComparison.OrdinalIgnoreCase));

    private static double? ParsePageValue(string? value) =>
        value is null ? null : int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    private static Page NewPage(double? page, double? size, bool matches) =>
        new()
        {
            Number = page ?? 0,
            Size = size ?? 20,
            TotalElements = matches ? 1 : 0,
            TotalPages = matches ? 1 : 0,
        };

    private static Enhet NewEnhet() => new() { Organisasjonsnummer = EnhetNumber, Navn = Name };

    private static Underenhet NewUnderenhet() =>
        new()
        {
            Organisasjonsnummer = UnderenhetNumber,
            Navn = Name,
            OverordnetEnhet = EnhetNumber,
        };
}
