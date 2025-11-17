using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Validation.Extensions;
using Shouldly;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test;

public class QueryExtensionsTests
{
    private VerifySettings _verifySettings = new();

    public QueryExtensionsTests()
    {
        _verifySettings.UseDirectory("TestData/Snapshots");
    }

    [Theory]
    [InlineData("123456789", true)]
    [InlineData("987654321", true)]
    [InlineData("nibokstav", false)]
    [InlineData("12345678", false)]
    [InlineData("1234567890", false)]
    public void ValidateOrgnummerOrThrow_ThrowsWhenInvalid(string orgnummer, bool expected)
    {
        // Act & Assert
        if (expected)
        {
            orgnummer.ValidateOrgnummerOrThrow(nameof(orgnummer));
        }
        else
        {
            var result = () => orgnummer.ValidateOrgnummerOrThrow(nameof(orgnummer));
            result.ShouldThrow<ArgumentException>();
        }
    }

    [Theory]
    [InlineData("123456789", true)]
    [InlineData("987654321", true)]
    [InlineData("nibokstav", false)]
    [InlineData("12345678", false)]
    [InlineData("1234567890", false)]
    public void IsValidOrgnummer_ReturnsCorrectResult(string orgnummer, bool expected)
    {
        // Act
        var result = orgnummer.IsValidOrgnummer();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public async Task SearchParameters_ToMap_MapsCorrectly()
    {
        // Arrange
        var query = new SearchEnheterQuery
        {
            Navn = "Test",
            Organisasjonsnummer = ["123456789", "987654321"],
            Organisasjonsform = ["AS", "ENK"],
            OverordnetEnhetOrganisasjonsnummer = "123456789",
            SortBy = "navn",
            StrictSearch = true,
            SortDirection = SearchEnheterQuery.Sort.Asc,
        };

        // Act & Assert
        var parameterMap = query.ToMap();

        await Verify(parameterMap, _verifySettings);
    }

    [Fact]
    public async Task SearchParameters_DefaultValues_ToMap_MapsCorrectly()
    {
        // Arrange
        var query = new SearchEnheterQuery();

        // Act & Assert
        var parameterMap = query.ToMap();

        await Verify(parameterMap, _verifySettings);
    }

    [Fact]
    public async Task Pagination_ToMap_MapsCorrectly()
    {
        // Arrange
        var pagination = new Pagination { Page = 1, Size = 20 };

        // Act & Assert
        var parameterMap = pagination.ToMap();

        await Verify(parameterMap, _verifySettings);
    }

    [Fact]
    public async Task Pagination_DefaultValues_ToMap_MapsCorrectly()
    {
        // Arrange
        var pagination = new Pagination();

        // Act & Assert
        var parameterMap = pagination.ToMap();

        await Verify(parameterMap, _verifySettings);
    }

    [Fact]
    public async Task GetOppdateringerQuery_ToMap_MapsCorrectly()
    {
        // Arrange
        var query = new GetOppdateringerQuery
        {
            Dato = DateTime.Now,
            Organisasjonsnummer = ["123456789", "987654321"],
            Oppdateringsid = 69,
        };

        // Act & Assert
        var parameterMap = query.ToMap();

        await Verify(parameterMap, _verifySettings);
    }

    [Fact]
    public async Task GetOppdateringerQuery_DefaultValues_ToMap_MapsCorrectly()
    {
        // Arrange
        var query = new GetOppdateringerQuery() { Dato = DateTime.Now };

        // Act & Assert
        var parameterMap = query.ToMap();

        await Verify(parameterMap, _verifySettings);
    }
}
