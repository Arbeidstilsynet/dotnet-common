using Arbeidstilsynet.Common.Enhetsregisteret.Extensions;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Response;
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
                Arg.Is<SearchEnheterQuery>(q =>
                    q.OverordnetEnhetOrganisasjonsnummer == "123456789"
                ),
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
        var result = new PaginationResult<Enhet>()
        {
            PageIndex = 0,
            Elements = [new Enhet()],
            TotalElements = 3,
            PageSize = 1,
        };
        _enhetsregisteret
            .SearchEnheter(Arg.Any<SearchEnheterQuery>(), Arg.Is<Pagination>(p => p.Page == 0))
            .Returns(result);
        _enhetsregisteret
            .SearchEnheter(Arg.Any<SearchEnheterQuery>(), Arg.Is<Pagination>(p => p.Page == 1))
            .Returns(result with { PageIndex = 1 });
        _enhetsregisteret
            .SearchEnheter(Arg.Any<SearchEnheterQuery>(), Arg.Is<Pagination>(p => p.Page == 2))
            .Returns(result with { PageIndex = 2 });

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
        var result = new PaginationResult<Underenhet>()
        {
            PageIndex = 0,
            Elements = [new Underenhet(), new Underenhet()],
            TotalElements = 4,
            PageSize = 2,
        };
        _enhetsregisteret
            .SearchUnderenheter(Arg.Any<SearchEnheterQuery>(), Arg.Is<Pagination>(p => p.Page == 0))
            .Returns(result);
        _enhetsregisteret
            .SearchUnderenheter(Arg.Any<SearchEnheterQuery>(), Arg.Is<Pagination>(p => p.Page == 1))
            .Returns(result with { PageIndex = 1 });

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
        var result = new PaginationResult<Oppdatering>()
        {
            PageIndex = 0,
            Elements = [new Oppdatering(), new Oppdatering()],
            TotalElements = 5,
            PageSize = 2,
        };
        _enhetsregisteret
            .GetOppdateringerEnheter(
                Arg.Any<GetOppdateringerQuery>(),
                Arg.Is<Pagination>(p => p.Page == 0)
            )
            .Returns(result);
        _enhetsregisteret
            .GetOppdateringerEnheter(
                Arg.Any<GetOppdateringerQuery>(),
                Arg.Is<Pagination>(p => p.Page == 1)
            )
            .Returns(result with { PageIndex = 1 });
        _enhetsregisteret
            .GetOppdateringerEnheter(
                Arg.Any<GetOppdateringerQuery>(),
                Arg.Is<Pagination>(p => p.Page == 2)
            )
            .Returns(result with { PageIndex = 2, Elements = [new Oppdatering()] });

        var query = new GetOppdateringerQuery { Dato = DateTime.Now };

        var results = new List<Oppdatering>();

        await foreach (var oppdatering in _enhetsregisteret.GetOppdateringerEnheter(query))
        {
            results.Add(oppdatering);
        }

        results.Count.ShouldBe(5);
    }

    [Fact]
    public async Task GetOppdateringerUnderenheter_EnumeratesASinglePage()
    {
        var result = new PaginationResult<Oppdatering>()
        {
            PageIndex = 0,
            Elements = [new Oppdatering(), new Oppdatering(), new Oppdatering(), new Oppdatering()],
            TotalElements = 4,
            PageSize = 1,
        };
        _enhetsregisteret
            .GetOppdateringerUnderenheter(
                Arg.Any<GetOppdateringerQuery>(),
                Arg.Is<Pagination>(p => p.Page == 0)
            )
            .Returns(result);

        var query = new GetOppdateringerQuery { Dato = DateTime.Now };

        var results = new List<Oppdatering>();

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

        var results = new List<int>();

        await foreach (
            var result in EnhetsregisteretExtensions.EnumeratePaginatedElements(FetchPage)
        )
        {
            results.Add(result);
        }

        results.ShouldBe(elements);
        return;

        Task<PaginationResult<int>?> FetchPage(Pagination pagination)
        {
            var startIndex = (int)pagination.Page * pageSize;

            return Task.FromResult<PaginationResult<int>?>(
                new PaginationResult<int>()
                {
                    PageIndex = pagination.Page,
                    Elements = elements.Skip(startIndex).Take(pageSize),
                    TotalElements = totalElements,
                    PageSize = pageSize,
                }
            );
        }
    }
}
