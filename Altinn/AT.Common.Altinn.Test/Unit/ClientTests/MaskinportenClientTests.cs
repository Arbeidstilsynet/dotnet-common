using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Test.Unit.Setup;
using Shouldly;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit.ClientTests;

public class MaskinportenClientTests : TestBed<AltinnApiTestFixture>
{
    private readonly IMaskinportenClient _sut;

    public MaskinportenClientTests(
        ITestOutputHelper testOutputHelper,
        AltinnApiTestFixture altinnApiTestFixture
    )
        : base(testOutputHelper, altinnApiTestFixture)
    {
        _sut = altinnApiTestFixture.GetService<IMaskinportenClient>(testOutputHelper)!;
    }

    [Fact]
    public async Task ExchangeToken_WhenCalledWithMaskinportenToken_ReturnsJwtToken()
    {
        //arrange
        //act
        var result = await _sut.GetToken();

        //assert
        result.ShouldBeEquivalentTo(AltinnApiTestFixture.SampleMaskinportenTokenResponse);
    }
}
