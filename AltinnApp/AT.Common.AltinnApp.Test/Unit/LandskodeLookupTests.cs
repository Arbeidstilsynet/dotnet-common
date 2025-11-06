using Arbeidstilsynet.Common.AltinnApp.Implementation;
using Arbeidstilsynet.Common.AltinnApp.Model;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

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
            new object[] { "BEL", new Landskode("Belgium", "+32", "BE", "BEL") },
            new object[] { "BGR", new Landskode("Bulgaria", "+359", "BG", "BGR") },
            new object[] { "DNK", new Landskode("Denmark", "+45", "DK", "DNK") },
            new object[] { "EST", new Landskode("Estonia", "+372", "EE", "EST") },
            new object[] { "FIN", new Landskode("Finland", "+358", "FI", "FIN") },
            new object[] { "FRA", new Landskode("France", "+33", "FR", "FRA") },
            new object[] { "GRC", new Landskode("Greece", "+30", "GR", "GRC") },
            new object[] { "IRL", new Landskode("Ireland", "+353", "IE", "IRL") },
            new object[] { "ISL", new Landskode("Iceland", "+354", "IS", "ISL") },
            new object[] { "ITA", new Landskode("Italy", "+39", "IT", "ITA") },
            new object[] { "HRV", new Landskode("Croatia", "+385", "HR", "HRV") },
            new object[] { "CYP", new Landskode("Cyprus", "+357", "CY", "CYP") },
            new object[] { "LVA", new Landskode("Latvia", "+371", "LV", "LVA") },
            new object[] { "LIE", new Landskode("Liechtenstein", "+423", "LI", "LIE") },
            new object[] { "LTU", new Landskode("Lithuania", "+370", "LT", "LTU") },
            new object[] { "LUX", new Landskode("Luxembourg", "+352", "LU", "LUX") },
            new object[] { "MLT", new Landskode("Malta", "+356", "MT", "MLT") },
            new object[] { "NLD", new Landskode("Netherlands", "+31", "NL", "NLD") },
            new object[] { "NOR", new Landskode("Norway", "+47", "NO", "NOR") },
            new object[] { "POL", new Landskode("Poland", "+48", "PL", "POL") },
            new object[] { "PRT", new Landskode("Portugal", "+351", "PT", "PRT") },
            new object[] { "ROU", new Landskode("Romania", "+40", "RO", "ROU") },
            new object[] { "SVK", new Landskode("Slovakia", "+421", "SK", "SVK") },
            new object[] { "SVN", new Landskode("Slovenia", "+386", "SI", "SVN") },
            new object[] { "ESP", new Landskode("Spain", "+34", "ES", "ESP") },
            new object[] { "SWE", new Landskode("Sweden", "+46", "SE", "SWE") },
            new object[] { "CZE", new Landskode("Czechia", "+420", "CZ", "CZE") },
            new object[] { "DEU", new Landskode("Germany", "+49", "DE", "DEU") },
            new object[] { "HUN", new Landskode("Hungary", "+36", "HU", "HUN") },
            new object[] { "AUT", new Landskode("Austria", "+43", "AT", "AUT") },
            new object[] { "CHE", new Landskode("Switzerland", "+41", "CH", "CHE") },
            new object[] { "GBR", new Landskode("United Kingdom", "+44", "GB", "GBR") },
        };

    [Fact]
    public async Task GetLandskoder_Returns_Alle_Landskoder()
    {
        const int antallLandskoder = 253;

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
