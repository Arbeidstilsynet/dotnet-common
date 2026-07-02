using Arbeidstilsynet.Common.Enhetsregisteret.Extensions;
using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Models;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test;

public class EnhetsregisteretExtensionsTests
{
    private readonly IEnhetsregisteret _enhetsregisteret = Substitute.For<IEnhetsregisteret>();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("69")]
    [InlineData("bokstaver")]
    public async Task GetUnderenheter_InvalidOverordnetEnhet_ThrowsArgumentException(
        string? organisasjonsnummer
    )
    {
        // Act
        var act = () => _enhetsregisteret.GetUnderenheterByHovedenhet(organisasjonsnummer!);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetUnderenheter_ValidAntall_CallsSearchUnderenhteterCorrectly()
    {
        // Act
        _ = await _enhetsregisteret.GetUnderenheterByHovedenhet("123456789");

        // Assert
        await _enhetsregisteret
            .Received(1)
            .SearchUnderenheter(
                Arg.Is<SearchEnheterQuery>(q => q.OverordnetEnhetOrganisasjonsnummer == "123456789"),
                Arg.Any<Pagination>()
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
            .SearchUnderenheter(Arg.Any<SearchEnheterQuery>(), Arg.Any<Pagination>());
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
            .SearchEnheter(Arg.Any<SearchEnheterQuery>(), Arg.Any<Pagination>());
    }

    [Fact]
    public async Task GetEnheter_ValidAntall_CallsSearchEnheterCorrectly()
    {
        // Act
        _ = await _enhetsregisteret.GetEnheter(["123456789", "987654321"]);

        // Assert
        await _enhetsregisteret
            .Received(1)
            .SearchEnheter(
                Arg.Is<SearchEnheterQuery>(q =>
                    q.Organisasjonsnummer.SequenceEqual(
                        new List<string>() { "123456789", "987654321" }
                    )
                ),
                Arg.Any<Pagination>()
            );
    }

    [Fact]
    public async Task SearchEnheter_EnumeratesAllPages()
    {
        Enheter Page() => BuildEnheter([new Enhet()], pageSize: 1, totalElements: 3);

        _enhetsregisteret
            .SearchEnheter(Arg.Any<SearchEnheterQuery>(), Arg.Is<Pagination>(p => p.Page == 0))
            .Returns(Page());
        _enhetsregisteret
            .SearchEnheter(Arg.Any<SearchEnheterQuery>(), Arg.Is<Pagination>(p => p.Page == 1))
            .Returns(Page());
        _enhetsregisteret
            .SearchEnheter(Arg.Any<SearchEnheterQuery>(), Arg.Is<Pagination>(p => p.Page == 2))
            .Returns(Page());

        var query = new SearchEnheterQuery();

        var results = new List<Enhet>();

        await foreach (var enhet in _enhetsregisteret.SearchEnheter(query))
        {
            results.Add(enhet);
        }

        results.Count.ShouldBe(3);
    }

    [Fact]
    public async Task SearchUnderenheter_EnumeratesAllPages()
    {
        Underenheter Page() =>
            BuildUnderenheter([new Underenhet(), new Underenhet()], pageSize: 2, totalElements: 4);

        _enhetsregisteret
            .SearchUnderenheter(Arg.Any<SearchEnheterQuery>(), Arg.Is<Pagination>(p => p.Page == 0))
            .Returns(Page());
        _enhetsregisteret
            .SearchUnderenheter(Arg.Any<SearchEnheterQuery>(), Arg.Is<Pagination>(p => p.Page == 1))
            .Returns(Page());

        var query = new SearchEnheterQuery();

        var results = new List<Underenhet>();

        await foreach (var enhet in _enhetsregisteret.SearchUnderenheter(query))
        {
            results.Add(enhet);
        }

        results.Count.ShouldBe(4);
    }

    [Fact]
    public async Task GetOppdateringerEnheter_EnumeratesFinalPartialPage()
    {
        _enhetsregisteret
            .GetOppdateringerEnheter(
                Arg.Any<GetOppdateringerQuery>(),
                Arg.Is<Pagination>(p => p.Page == 0)
            )
            .Returns(
                BuildOppdateringerEnheter(
                    [new OppdateringerEnhet(), new OppdateringerEnhet()],
                    pageSize: 2,
                    totalElements: 5
                )
            );
        _enhetsregisteret
            .GetOppdateringerEnheter(
                Arg.Any<GetOppdateringerQuery>(),
                Arg.Is<Pagination>(p => p.Page == 1)
            )
            .Returns(
                BuildOppdateringerEnheter(
                    [new OppdateringerEnhet(), new OppdateringerEnhet()],
                    pageSize: 2,
                    totalElements: 5
                )
            );
        _enhetsregisteret
            .GetOppdateringerEnheter(
                Arg.Any<GetOppdateringerQuery>(),
                Arg.Is<Pagination>(p => p.Page == 2)
            )
            .Returns(
                BuildOppdateringerEnheter(
                    [new OppdateringerEnhet()],
                    pageSize: 2,
                    totalElements: 5
                )
            );

        var query = new GetOppdateringerQuery { Dato = DateTime.Now };

        var results = new List<OppdateringerEnhet>();

        await foreach (var oppdatering in _enhetsregisteret.GetOppdateringerEnheter(query))
        {
            results.Add(oppdatering);
        }

        results.Count.ShouldBe(5);
    }

    [Fact]
    public async Task GetOppdateringerUnderenheter_EnumeratesASinglePage()
    {
        _enhetsregisteret
            .GetOppdateringerUnderenheter(
                Arg.Any<GetOppdateringerQuery>(),
                Arg.Is<Pagination>(p => p.Page == 0)
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

        var query = new GetOppdateringerQuery { Dato = DateTime.Now };

        var results = new List<OppdateringerUnderenhet>();

        await foreach (var oppdatering in _enhetsregisteret.GetOppdateringerUnderenheter(query))
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

        Task<IPaginatedResponse<int>?> FetchPage(Pagination pagination)
        {
            var startIndex = (int)pagination.Page * pageSize;

            return Task.FromResult<IPaginatedResponse<int>?>(
                new PaginatedResponse<int>(
                    elements.Skip(startIndex).Take(pageSize).ToList(),
                    totalPages
                )
            );
        }
    }

    [Fact]
    public async Task EnumeratePaginatedElements_StopsAtMaxSearchResultSize()
    {
        // EnumeratePaginatedElements drives pagination with a fixed page size of 1000.
        // The page extent guard ((page + 1) * 1000) exceeds Constants.MaxSearchResultSize (10_000)
        // at page 10, so enumeration stops after fetching pages 0..9 even though more pages exist.
        var callCount = 0;

        var results = new List<int>();

        await foreach (
            var result in EnhetsregisteretExtensions.EnumeratePaginatedElements(FetchPage)
        )
        {
            results.Add(result);
        }

        callCount.ShouldBe(10);
        results.Count.ShouldBe(10);

        return;

        Task<IPaginatedResponse<int>?> FetchPage(Pagination pagination)
        {
            callCount++;
            return Task.FromResult<IPaginatedResponse<int>?>(
                new PaginatedResponse<int>([1], TotalPages: 100)
            );
        }
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
