using Altinn.App.Core.Infrastructure.Clients.Events;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Test.Setup;
using ProtoBuf.Meta;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

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
            Model.Api.AuthenticationTokenProvider.Maskinporten,
            "dummyBearerToken"
        );

        //assert
        result.ShouldBe(AltinnApiTestFixture.SampleJwtToken);
    }
}
