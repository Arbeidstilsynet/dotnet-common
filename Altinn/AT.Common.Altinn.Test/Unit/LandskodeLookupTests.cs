using Arbeidstilsynet.Common.Altinn.Implementation;
using Arbeidstilsynet.Common.Altinn.Model;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class LandskodeLookupTests
{
    private readonly LandskodeLookup _sut;

    public LandskodeLookupTests()
    {
        _sut = new LandskodeLookup();
    }

    public static IEnumerable<object[]> EØSLand =>
        // Kilde: https://www.udi.no/ord-og-begreper/eueos-borger/
        new List<object[]>
        {
            new object[] { "BEL", new Landskode("Belgium", "+32") },
            new object[] { "BGR", new Landskode("Bulgaria", "+359") },
            new object[] { "DNK", new Landskode("Denmark", "+45") },
            new object[] { "EST", new Landskode("Estonia", "+372") },
            new object[] { "FIN", new Landskode("Finland", "+358") },
            new object[] { "FRA", new Landskode("France", "+33") },
            new object[] { "GRC", new Landskode("Greece", "+30") },
            new object[] { "IRL", new Landskode("Ireland", "+353") },
            new object[] { "ISL", new Landskode("Iceland", "+354") },
            new object[] { "ITA", new Landskode("Italy", "+39") },
            new object[] { "HRV", new Landskode("Croatia", "+385") },
            new object[] { "CYP", new Landskode("Cyprus", "+357") },
            new object[] { "LVA", new Landskode("Latvia", "+371") },
            new object[] { "LIE", new Landskode("Liechtenstein", "+423") },
            new object[] { "LTU", new Landskode("Lithuania", "+370") },
            new object[] { "LUX", new Landskode("Luxembourg", "+352") },
            new object[] { "MLT", new Landskode("Malta", "+356") },
            new object[] { "NLD", new Landskode("Netherlands", "+31") },
            new object[] { "NOR", new Landskode("Norway", "+47") },
            new object[] { "POL", new Landskode("Poland", "+48") },
            new object[] { "PRT", new Landskode("Portugal", "+351") },
            new object[] { "ROU", new Landskode("Romania", "+40") },
            new object[] { "SVK", new Landskode("Slovakia", "+421") },
            new object[] { "SVN", new Landskode("Slovenia", "+386") },
            new object[] { "ESP", new Landskode("Spain", "+34") },
            new object[] { "SWE", new Landskode("Sweden", "+46") },
            new object[] { "CZE", new Landskode("Czech Republic", "+420") },
            new object[] { "DEU", new Landskode("Germany", "+49") },
            new object[] { "HUN", new Landskode("Hungary", "+36") },
            new object[] { "AUT", new Landskode("Austria", "+43") },
            new object[] { "CHE", new Landskode("Switzerland", "+41") },
            new object[] { "GBR", new Landskode("United Kingdom", "+44") },
        };

    [Fact]
    public async Task GetLandskoder_Returns_Alle_Landskoder()
    {
        const int antallLandskoder = 238;

        var landskoder = (await _sut.GetLandskoder()).ToList();

        landskoder.ShouldNotBeEmpty();
        landskoder.Count.ShouldBe(antallLandskoder);
    }

    [Theory]
    [MemberData(nameof(EØSLand))]
    public async Task GetLandskode_Returns_Landskode_ForEØSLand(
        string landskode,
        Landskode expectedData
    )
    {
        var result = await _sut.GetLandskode(landskode);

        result.ShouldBe(expectedData);
    }

    [Fact]
    public async Task GetLandskode_WhenLandDoesNotExist_ReturnsNull()
    {
        var result = await _sut.GetLandskode("XXX");

        result.ShouldBeNull();
    }
}
