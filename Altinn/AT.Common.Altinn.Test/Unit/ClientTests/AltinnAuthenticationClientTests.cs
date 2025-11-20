using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Test.Unit.Setup;
using Shouldly;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit.ClientTests;

public class AltinnAuthenticationClientTests : TestBed<AltinnApiTestFixture>
{
    private readonly IAltinnAuthenticationClient _sut;

    public AltinnAuthenticationClientTests(
        ITestOutputHelper testOutputHelper,
        AltinnApiTestFixture altinnApiTestFixture
    )
        : base(testOutputHelper, altinnApiTestFixture)
    {
        _sut = altinnApiTestFixture.GetService<IAltinnAuthenticationClient>(testOutputHelper)!;
    }

    [Fact]
    public async Task ExchangeToken_WhenCalledWithMaskinportenToken_ReturnsJwtToken()
    {
        //arrange
        //act
        var result = await _sut.ExchangeToken(
            "dummyBearerToken",
            Model.Api.AuthenticationTokenProvider.Maskinporten
        );

        //assert
        result.ShouldBe(AltinnApiTestFixture.SampleJwtToken);
    }
}
