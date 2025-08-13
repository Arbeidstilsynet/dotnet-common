using Altinn.App.Core.Infrastructure.Clients.Events;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Test.Setup;
using ProtoBuf.Meta;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

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
        var result = await _sut.GetToken(AltinnApiTestFixture.SampleJwtToken);

        //assert
        result.ShouldBeEquivalentTo(AltinnApiTestFixture.SampleMaskinportenTokenResponse);
    }
}
