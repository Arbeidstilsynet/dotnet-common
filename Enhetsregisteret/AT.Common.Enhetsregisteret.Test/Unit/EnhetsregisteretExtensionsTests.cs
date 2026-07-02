using Arbeidstilsynet.Common.Enhetsregisteret.Extensions;
using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Models;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using NSubstitute;
using Shouldly;
using EnheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Enheter.EnheterRequestBuilder.EnheterRequestBuilderGetQueryParameters;
using OppdateringerEnheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Oppdateringer.Enheter.EnheterRequestBuilder.EnheterRequestBuilderGetQueryParameters;
using OppdateringerUnderenheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Oppdateringer.Underenheter.UnderenheterRequestBuilder.UnderenheterRequestBuilderGetQueryParameters;
using UnderenheterQueryParameters = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Underenheter.UnderenheterRequestBuilder.UnderenheterRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test;

public class EnhetsregisteretExtensionsTests
{
    private readonly IEnhetsregisteret _enhetsregisteret = Substitute.For<IEnhetsregisteret>();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("69")]
    [InlineData("bokstaver")]
    public async Task GetUnderenheterByHovedenhet_InvalidOverordnetEnhet_ThrowsArgumentException(
        string? organisasjonsnummer
    )
    {
        // Act
        var act = () => _enhetsregisteret.GetUnderenheterByHovedenhet(organisasjonsnummer!);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetUnderenheterByHovedenhet_ValidOrgnummer_ConfiguresOverordnetEnhet()
    {
        // Act
        _ = await _enhetsregisteret.GetUnderenheterByHovedenhet("123456789");

        // Assert
        await _enhetsregisteret
            .Received(1)
            .SearchUnderenheter(
                Arg.Is<Action<UnderenheterQueryParameters>>(configure =>
                    Applied(configure).OverordnetEnhet == "123456789"
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task GetUnderenheter_Organisasjonsnummer_IsEmpty()
    {
        // Arrange
        var orgnummer = Enumerable.Empty<string>();

        // Act
        var result = await _enhetsregisteret.GetUnderenheter(orgnummer);

        // Assert
        result.ShouldBeEmpty();
        await _enhetsregisteret
            .DidNotReceive()
            .SearchUnderenheter(
                Arg.Any<Action<UnderenheterQueryParameters>>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task GetUnderenheter_ValidOrgnummer_ConfiguresOrganisasjonsnummer()
    {
        // Act
        _ = await _enhetsregisteret.GetUnderenheter(["123456789", "987654321"]);

        // Assert
        await _enhetsregisteret
            .Received(1)
            .SearchUnderenheter(
                Arg.Is<Action<UnderenheterQueryParameters>>(configure =>
                    Applied(configure)
                        .Organisasjonsnummer!.SequenceEqual(new[] { "123456789", "987654321" })
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task GetEnheter_Organisasjonsnummer_IsEmpty()
    {
        // Arrange
        var orgnummer = Enumerable.Empty<string>();

        // Act
        var result = await _enhetsregisteret.GetEnheter(orgnummer);

        // Assert
        result.ShouldBeEmpty();
        await _enhetsregisteret
            .DidNotReceive()
            .SearchEnheter(Arg.Any<Action<EnheterQueryParameters>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetEnheter_ValidOrgnummer_ConfiguresOrganisasjonsnummer()
    {
        // Act
        _ = await _enhetsregisteret.GetEnheter(["123456789", "987654321"]);

        // Assert
        await _enhetsregisteret
            .Received(1)
            .SearchEnheter(
                Arg.Is<Action<EnheterQueryParameters>>(configure =>
                    Applied(configure)
                        .Organisasjonsnummer!.SequenceEqual(new[] { "123456789", "987654321" })
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task EnumerateEnheter_EnumeratesAllPages()
    {
        Enheter Page() => BuildEnheter([new Enhet()], pageSize: 1, totalElements: 3);

        _enhetsregisteret
            .SearchEnheter(Arg.Any<Action<EnheterQueryParameters>>(), Arg.Any<CancellationToken>())
            .Returns(Page(), Page(), Page());

        var results = new List<Enhet>();

        await foreach (var enhet in _enhetsregisteret.EnumerateEnheter(_ => { }))
        {
            results.Add(enhet);
        }

        results.Count.ShouldBe(3);
    }

    [Fact]
    public async Task EnumerateUnderenheter_EnumeratesAllPages()
    {
        Underenheter Page() =>
            BuildUnderenheter([new Underenhet(), new Underenhet()], pageSize: 2, totalElements: 4);

        _enhetsregisteret
            .SearchUnderenheter(
                Arg.Any<Action<UnderenheterQueryParameters>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Page(), Page());

        var results = new List<Underenhet>();

        await foreach (var enhet in _enhetsregisteret.EnumerateUnderenheter(_ => { }))
        {
            results.Add(enhet);
        }

        results.Count.ShouldBe(4);
    }

    [Fact]
    public async Task EnumerateOppdateringerEnheter_EnumeratesFinalPartialPage()
    {
        _enhetsregisteret
            .GetOppdateringerEnheter(
                Arg.Any<Action<OppdateringerEnheterQueryParameters>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                BuildOppdateringerEnheter(
                    [new OppdateringerEnhet(), new OppdateringerEnhet()],
                    pageSize: 2,
                    totalElements: 5
                ),
                BuildOppdateringerEnheter(
                    [new OppdateringerEnhet(), new OppdateringerEnhet()],
                    pageSize: 2,
                    totalElements: 5
                ),
                BuildOppdateringerEnheter([new OppdateringerEnhet()], pageSize: 2, totalElements: 5)
            );

        var results = new List<OppdateringerEnhet>();

        await foreach (
            var oppdatering in _enhetsregisteret.EnumerateOppdateringerEnheter(_ => { })
        )
        {
            results.Add(oppdatering);
        }

        results.Count.ShouldBe(5);
    }

    [Fact]
    public async Task EnumerateOppdateringerUnderenheter_EnumeratesASinglePage()
    {
        _enhetsregisteret
            .GetOppdateringerUnderenheter(
                Arg.Any<Action<OppdateringerUnderenheterQueryParameters>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                BuildOppdateringerUnderenheter(
                    [
                        new OppdateringerUnderenhet(),
                        new OppdateringerUnderenhet(),
                        new OppdateringerUnderenhet(),
                        new OppdateringerUnderenhet(),
                    ],
                    pageSize: 4,
                    totalElements: 4
                )
            );

        var results = new List<OppdateringerUnderenhet>();

        await foreach (
            var oppdatering in _enhetsregisteret.EnumerateOppdateringerUnderenheter(_ => { })
        )
        {
            results.Add(oppdatering);
        }

        results.Count.ShouldBe(4);
    }

    [Theory]
    [InlineData(1, 4)]
    [InlineData(2, 5)]
    [InlineData(3, 2)]
    [InlineData(3, 6)]
    public async Task EnumeratePaginatedElements_EnumeratesAllElements(
        int pageSize,
        int totalElements
    )
    {
        var elements = Enumerable.Range(1, totalElements).ToList();

        var totalPages = (long)Math.Ceiling((double)totalElements / pageSize);

        var results = new List<int>();

        await foreach (
            var result in EnhetsregisteretExtensions.EnumeratePaginatedElements(FetchPage)
        )
        {
            results.Add(result);
        }

        results.ShouldBe(elements);
        return;

        Task<IPaginatedResponse<int>?> FetchPage(int page)
        {
            var startIndex = page * pageSize;

            return Task.FromResult<IPaginatedResponse<int>?>(
                new PaginatedResponse<int>(
                    elements.Skip(startIndex).Take(pageSize).ToList(),
                    totalPages
                )
            );
        }
    }

    private static T Applied<T>(Action<T> configure)
        where T : new()
    {
        var queryParameters = new T();
        configure(queryParameters);
        return queryParameters;
    }

    private static Enheter BuildEnheter(
        IEnumerable<Enhet> elements,
        long pageSize,
        long totalElements
    ) =>
        new()
        {
            Embedded = new Enheter_embedded { Enheter = elements.ToList() },
            Page = BuildPage(pageSize, totalElements),
        };

    private static Underenheter BuildUnderenheter(
        IEnumerable<Underenhet> elements,
        long pageSize,
        long totalElements
    ) =>
        new()
        {
            Embedded = new Underenheter_embedded { Underenheter = elements.ToList() },
            Page = BuildPage(pageSize, totalElements),
        };

    private static OppdateringerEnheter BuildOppdateringerEnheter(
        IEnumerable<OppdateringerEnhet> elements,
        long pageSize,
        long totalElements
    ) =>
        new()
        {
            Embedded = new OppdateringerEnheter_embedded { OppdaterteEnheter = elements.ToList() },
            Page = BuildPage(pageSize, totalElements),
        };

    private static OppdateringerUnderenheter BuildOppdateringerUnderenheter(
        IEnumerable<OppdateringerUnderenhet> elements,
        long pageSize,
        long totalElements
    ) =>
        new()
        {
            Embedded = new OppdateringerUnderenheter_embedded
            {
                OppdaterteUnderenheter = elements.ToList(),
            },
            Page = BuildPage(pageSize, totalElements),
        };

    private static Page BuildPage(long pageSize, long totalElements) =>
        new() { Size = pageSize, TotalElements = totalElements };
}
