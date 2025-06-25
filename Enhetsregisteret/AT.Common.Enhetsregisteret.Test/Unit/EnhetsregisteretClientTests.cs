using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;
using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test;

public class EnhetsregisteretClientUnitTests
{
    private readonly EnhetsregisteretClient _sut;

    private readonly IMemoryCache _memoryCache = Substitute.For<IMemoryCache>();

    private readonly EnhetsregisteretConfig _cacheOptions = new(
        cacheOptions: new CacheOptions { Disabled = false, ExpirationTime = TimeSpan.FromDays(1) }
    );

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
            Substitute.For<ILogger<EnhetsregisteretClient>>()
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
        result!.Elements.ShouldBeEquivalentTo(new List<Underenhet> { underenhet });
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
}
