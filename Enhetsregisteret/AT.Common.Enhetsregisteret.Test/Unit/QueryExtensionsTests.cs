using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Shouldly;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test;

public class QueryExtensionsTests
{
    private VerifySettings _verifySettings = new ();

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
    public void ValidateOrgnummerOrThrow_ValidOrgnummer_DoesNotThrow(string orgnummer, bool expected)
    {
        // Act & Assert
        if (expected)
        {
            orgnummer.ValidateOrgnummerOrThrow(nameof(orgnummer));
        }
        else
        {
            Assert.Throws<ArgumentException>(() => orgnummer.ValidateOrgnummerOrThrow(nameof(orgnummer)));
        }
    }

    [Fact]
    public async Task SearchParameters_ToMap_Mapscorrectly()
    {
        // Arrange
        var query = new SearchEnheterQuery
        {
            Navn = "Test",
            Organisasjonsnummer = new[] { "123456789", "987654321" },
            Organisasjonsform = new[] { "AS", "ENK" },
            OverordnetEnhetOrganisasjonsnummer = "123456789",
            SortBy = "navn",
            SortDirection = SearchEnheterQuery.Sort.Asc
        };

        // Act & Assert
        var parameterMap = query.ToMap();

        await Verify(parameterMap, _verifySettings);
    }
}