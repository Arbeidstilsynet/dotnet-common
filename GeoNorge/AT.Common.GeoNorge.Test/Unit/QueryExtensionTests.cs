using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;

namespace Arbeidstilsynet.Common.GeoNorge.Adapters.Test.Unit;

public class QueryExtensionTests
{
    private readonly VerifySettings _verifySettings = new VerifySettings();
    
    public QueryExtensionTests()
    {
        _verifySettings.UseDirectory("TestData/Snapshots");
    }
    
    [Fact]
    public async Task TextSearchQuery_ToMap_MapsCorrectly()
    {
        // Arrange
        var query = new TextSearchQuery()
        {
            SearchTerm = "TestTerm",
            FuzzySearch = true,
            Adressenavn = "Testveien",
            Postnummer = "1234",
            Poststed = "Testby",
            Kommunenummer = "2510",
        };

        // Act & Assert
        var parameterMap = query.ToMap();

        await Verify(parameterMap, _verifySettings);
    }

    [Fact]
    public async Task TextSearchQuery_DefaultValues_ToMap_MapsCorrectly()
    {
        // Arrange
        var query = new TextSearchQuery()
        {
            SearchTerm = "SearchTerm",
        };

        // Act & Assert
        var parameterMap = query.ToMap();

        await Verify(parameterMap, _verifySettings);
    }

    [Fact]
    public async Task PointSearchQuery_ToMap_MapsCorrectly()
    {
        // Arrange
        var query = new PointSearchQuery()
        {
            Point = new Location()
            {
                Latitude = 4.2,
                Longitude = 4.2 
            },
            RadiusInMeters = 42
        };

        // Act & Assert
        var parameterMap = query.ToMap();

        await Verify(parameterMap, _verifySettings);
    }
    
    [Fact]
    public async Task Pagination_ToMap_MapsCorrectly()
    {
        // Arrange
        var pagination = new Pagination()
        {
            PageIndex = 1,
            PageSize = 20
        };

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
}