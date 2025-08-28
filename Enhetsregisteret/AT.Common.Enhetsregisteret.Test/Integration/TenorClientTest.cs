using System.Reflection;
using System.Threading.Tasks;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Tenor;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using Arbeidstilsynet.Common.Enhetsregisteret.Test.Integration.Fixture;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test.Unit;

public class TenorClientTests : TestBed<EnhetsregisterTenorTestFixture>
{
    private IEnhetsregisteret _sut;

    private const string HovedenhetOrgNr = "214246252";

    private const string UnderenhetOrgNr = "311672932";

    private readonly VerifySettings _verifySettings = new();

    public TenorClientTests(
        ITestOutputHelper testOutputHelper,
        EnhetsregisterTenorTestFixture fixture
    )
        : base(testOutputHelper, fixture)
    {
        _sut = fixture.GetService<IEnhetsregisteret>(testOutputHelper)!;
        _verifySettings.UseDirectory("TestData/Snapshots");
    }

    [Fact]
    public async Task GetEnhet_WhenCalledWithNotExistingOrgNr_ReturnsNull()
    {
        //act
        var result = await _sut.GetEnhet(UnderenhetOrgNr);
        //assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetEnhet_WhenCalledWithExistingOrgNr_ReturnsTenorEnhet()
    {
        //act
        var result = await _sut.GetEnhet(HovedenhetOrgNr);
        //assert
        await Verify(result, _verifySettings);
    }

    [Fact]
    public async Task GetUnderenhet_WhenCalledWithNotExistingOrgNr_ReturnsNull()
    {
        //act
        var result = await _sut.GetUnderenhet(HovedenhetOrgNr);
        //assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetUnderenhet_WhenCalledWithExistingOrgNr_ReturnsTenorEnhet()
    {
        //act
        var result = await _sut.GetUnderenhet(UnderenhetOrgNr);
        //assert
        await Verify(result, _verifySettings);
    }
}
