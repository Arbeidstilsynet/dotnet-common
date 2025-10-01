using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;
using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Validation;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test.Unit;

public class EnhetsregisteretClientUnitTests
{
    private readonly EnhetsregisteretClient _sut;

    private readonly IMemoryCache _memoryCache = Substitute.For<IMemoryCache>();

    private readonly EnhetsregisteretConfig _cacheOptions = new()
    {
        CacheOptions = new CacheOptions { Disabled = false },
    };
    
    private readonly List<IValidator> _validators = [
        new PaginationValidator(),
        new GetOppdateringerQueryValidator(),
        new SearchEnheterQueryValidator()
    ];

    public EnhetsregisteretClientUnitTests()
    {
        var httpClient = Substitute.For<HttpClient>();
        httpClient.BaseAddress = new Uri("http://localhost"); // This is just to prevent the implementation from throwing an exception
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        _sut = new EnhetsregisteretClient(
            httpClientFactory,
            _memoryCache,
            _cacheOptions,
            Substitute.For<ILogger<EnhetsregisteretClient>>(),
            _validators
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("69")]
    [InlineData("bokstaver")]
    public async Task GetEnhet_InvalidOrganisasjonsnummer_ThrowsArgumentException(
        string? organisasjonsnummer
    )
    {
        // Act
        var act = () => _sut.GetEnhet(organisasjonsnummer!);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetEnhet_ReturnsCachedEnhet()
    {
        // Arrange
        var enhet = new Enhet { Organisasjonsnummer = "123456789", Navn = "Test Enhet" };

        _memoryCache
            .TryGetValue(Arg.Any<string>(), out Arg.Any<Enhet?>())
            .Returns(x =>
            {
                x[1] = enhet; // Set the out parameter to the enhet
                return true;
            });

        // Act
        var result = await _sut.GetEnhet("123456789");

        // Assert
        result.ShouldBe(enhet);
    }

    [Fact]
    public async Task GetUndernhet_ReturnsCachedUnderenhet()
    {
        // Arrange
        var underenhet = new Underenhet
        {
            Organisasjonsnummer = "123456789",
            Navn = "Test Underenhet",
        };

        _memoryCache
            .TryGetValue(Arg.Any<string>(), out Arg.Any<Underenhet?>())
            .Returns(x =>
            {
                x[1] = underenhet; // Set the out parameter to the underenhet
                return true;
            });

        // Act
        var result = await _sut.GetUnderenhet("123456789");

        // Assert
        result.ShouldBe(underenhet);
    }

    [Fact]
    public async Task SearchUnderenheter_ValidQuery_ReturnsCachedUnderenheter()
    {
        // Arrange
        var searchParameters = new SearchEnheterQuery
        {
            Organisasjonsnummer = ["123456789"],
            Organisasjonsform = ["AS"],
        };

        var underenhet = new Underenhet
        {
            Organisasjonsnummer = "123456789",
            Navn = "Test Underenhet",
        };

        _memoryCache
            .TryGetValue(Arg.Any<string>(), out Arg.Any<UnderenheterResponse?>())
            .Returns(x =>
            {
                x[1] = new UnderenheterResponse()
                {
                    Embedded = new UnderenhetEmbeddedWrapper
                    {
                        Underenheter = new List<Underenhet> { underenhet },
                    },
                }; // Set the out parameter to the underenhet
                return true;
            });

        // Act
        var result = await _sut.SearchUnderenheter(searchParameters, new Pagination());

        // Assert
        result.ShouldNotBeNull();
        result.Elements.ShouldBeEquivalentTo(new List<Underenhet> { underenhet });
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("69")]
    [InlineData("bokstaver")]
    public async Task GetUnderenhet_InvalidOrgnummer_ThrowsArgumentException(string? orgnummer)
    {
        // Act
        var act = () => _sut.GetUnderenhet(orgnummer!);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }
    
    [Fact]
    public async Task Pagination_Over_10_000_ThrowsArgumentException()
    {
        // Arrange
        var searchParameters = new SearchEnheterQuery
        {
            Organisasjonsnummer = ["123456789"],
            Organisasjonsform = ["AS"],
        };

        var pagination = new Pagination
        {
            Size = 10001,
            Page = 0,
        };

        // Act
        var act = () => _sut.SearchUnderenheter(searchParameters, pagination);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }
    
    [Fact]
    public async Task SearchUnderenheter_NameLength_Over_180_ThrowsArgumentException()
    {
        // Arrange
        var searchParameters = new SearchEnheterQuery
        {
            Navn = new string('a', 181),
            Organisasjonsnummer = ["123456789"],
            Organisasjonsform = ["AS"],
        };

        // Act
        var act = () => _sut.SearchUnderenheter(searchParameters, new Pagination());

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SearchEnheter_Pagination_Over_10_000_ThrowsArgumentException()
    {
        // Arrange
        var searchParameters = new SearchEnheterQuery
        {
            Organisasjonsnummer = ["123456789"],
            Organisasjonsform = ["AS"],
        };

        var pagination = new Pagination
        {
            Size = 10001,
            Page = 0,
        };

        // Act
        var act = () => _sut.SearchEnheter(searchParameters, pagination);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SearchEnheter_NameLength_Over_180_ThrowsArgumentException()
    {
        // Arrange
        var searchParameters = new SearchEnheterQuery
        {
            Navn = new string('a', 181),
            Organisasjonsnummer = ["123456789"],
            Organisasjonsform = ["AS"],
        };

        // Act
        var act = () => _sut.SearchEnheter(searchParameters, new Pagination());

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("12345678")] // Too short
    [InlineData("1234567890")] // Too long
    public async Task SearchEnheter_InvalidOrganisasjonsnummer_ThrowsArgumentException(string invalidOrgnummer)
    {
        // Arrange
        var searchParameters = new SearchEnheterQuery
        {
            Organisasjonsnummer = [invalidOrgnummer],
            Organisasjonsform = ["AS"],
        };

        // Act
        var act = () => _sut.SearchEnheter(searchParameters, new Pagination());

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("12345678")] // Too short
    [InlineData("1234567890")] // Too long
    public async Task SearchEnheter_InvalidOverordnetEnhetOrganisasjonsnummer_ThrowsArgumentException(string invalidOrgnummer)
    {
        // Arrange
        var searchParameters = new SearchEnheterQuery
        {
            OverordnetEnhetOrganisasjonsnummer = invalidOrgnummer,
        };

        // Act
        var act = () => _sut.SearchEnheter(searchParameters, new Pagination());

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SearchEnheter_NegativePage_ThrowsArgumentException()
    {
        // Arrange
        var searchParameters = new SearchEnheterQuery
        {
            Organisasjonsnummer = ["123456789"],
        };

        var pagination = new Pagination
        {
            Size = 100,
            Page = -1,
        };

        // Act
        var act = () => _sut.SearchEnheter(searchParameters, pagination);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    // SearchUnderenheter validation tests (additional to existing ones)
    [Theory]
    [InlineData("12345678")] // Too short
    [InlineData("1234567890")] // Too long
    public async Task SearchUnderenheter_InvalidOrganisasjonsnummer_ThrowsArgumentException(string invalidOrgnummer)
    {
        // Arrange
        var searchParameters = new SearchEnheterQuery
        {
            Organisasjonsnummer = [invalidOrgnummer],
            Organisasjonsform = ["AS"],
        };

        // Act
        var act = () => _sut.SearchUnderenheter(searchParameters, new Pagination());

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("12345678")] // Too short
    [InlineData("1234567890")] // Too long
    public async Task SearchUnderenheter_InvalidOverordnetEnhetOrganisasjonsnummer_ThrowsArgumentException(string invalidOrgnummer)
    {
        // Arrange
        var searchParameters = new SearchEnheterQuery
        {
            OverordnetEnhetOrganisasjonsnummer = invalidOrgnummer,
        };

        // Act
        var act = () => _sut.SearchUnderenheter(searchParameters, new Pagination());

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SearchUnderenheter_NegativePage_ThrowsArgumentException()
    {
        // Arrange
        var searchParameters = new SearchEnheterQuery
        {
            Organisasjonsnummer = ["123456789"],
        };

        var pagination = new Pagination
        {
            Size = 100,
            Page = -1,
        };

        // Act
        var act = () => _sut.SearchUnderenheter(searchParameters, pagination);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    // GetOppdateringerEnheter validation tests
    [Fact]
    public async Task GetOppdateringerEnheter_Pagination_Over_10_000_ThrowsArgumentException()
    {
        // Arrange
        var query = new GetOppdateringerQuery()
        {
            Dato = DateTime.UtcNow,
        };
        var pagination = new Pagination
        {
            Size = 10001,
            Page = 0,
        };

        // Act
        var act = () => _sut.GetOppdateringerEnheter(query, pagination);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("12345678")] // Too short
    [InlineData("1234567890")] // Too long
    public async Task GetOppdateringerEnheter_InvalidOrganisasjonsnummer_ThrowsArgumentException(string invalidOrgnummer)
    {
        // Arrange
        var query = new GetOppdateringerQuery
        {
            Dato = DateTime.UtcNow,
            Organisasjonsnummer = [invalidOrgnummer],
        };

        // Act
        var act = () => _sut.GetOppdateringerEnheter(query, new Pagination());

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }


    // GetOppdateringerUnderenheter validation tests
    [Fact]
    public async Task GetOppdateringerUnderenheter_Pagination_Over_10_000_ThrowsArgumentException()
    {
        // Arrange
        var query = new GetOppdateringerQuery()
        {
            Dato = DateTime.UtcNow,
        };
        var pagination = new Pagination
        {
            Size = 10001,
            Page = 0,
        };

        // Act
        var act = () => _sut.GetOppdateringerUnderenheter(query, pagination);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    // Edge case tests for pagination calculation
    [Theory]
    [InlineData(9999, 1)] // Page 1, Size 9999 = 9999 * 2 = 19998 > 10000
    [InlineData(3334, 2)] // Page 2, Size 3334 = 3334 * 3 = 10002 > 10000
    public async Task SearchEnheter_PaginationCalculationEdgeCases_ThrowsArgumentException(int size, int page)
    {
        // Arrange
        var searchParameters = new SearchEnheterQuery
        {
            Organisasjonsnummer = ["123456789"],
        };

        var pagination = new Pagination
        {
            Size = size,
            Page = page,
        };

        // Act
        var act = () => _sut.SearchEnheter(searchParameters, pagination);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(9999, 1)] // Page 1, Size 9999 = 9999 * 2 = 19998 > 10000
    [InlineData(3334, 2)] // Page 2, Size 3334 = 3334 * 3 = 10002 > 10000
    public async Task SearchUnderenheter_PaginationCalculationEdgeCases_ThrowsArgumentException(int size, int page)
    {
        // Arrange
        var searchParameters = new SearchEnheterQuery();

        var pagination = new Pagination
        {
            Size = size,
            Page = page,
        };

        // Act
        var act = () => _sut.SearchUnderenheter(searchParameters, pagination);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(9999, 1)] // Page 1, Size 9999 = 9999 * 2 = 19998 > 10000
    [InlineData(3334, 2)] // Page 2, Size 3334 = 3334 * 3 = 10002 > 10000
    public async Task GetOppdateringerEnheter_PaginationCalculationEdgeCases_ThrowsArgumentException(int size, int page)
    {
        // Arrange
        var query = new GetOppdateringerQuery()
        {
            Dato = DateTime.UtcNow,
        };
        var pagination = new Pagination
        {
            Size = size,
            Page = page,
        };

        // Act
        var act = () => _sut.GetOppdateringerEnheter(query, pagination);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(9999, 1)] // Page 1, Size 9999 = 9999 * 2 = 19998 > 10000
    [InlineData(3334, 2)] // Page 2, Size 3334 = 3334 * 3 = 10002 > 10000
    public async Task GetOppdateringerUnderenheter_PaginationCalculationEdgeCases_ThrowsArgumentException(int size, int page)
    {
        // Arrange
        var query = new GetOppdateringerQuery()
        {
            Dato = DateTime.UtcNow,
        };
        var pagination = new Pagination
        {
            Size = size,
            Page = page,
        };

        // Act
        var act = () => _sut.GetOppdateringerUnderenheter(query, pagination);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
    }
}
