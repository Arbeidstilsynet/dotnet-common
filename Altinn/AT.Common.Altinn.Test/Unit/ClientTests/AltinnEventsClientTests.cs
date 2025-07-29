using Altinn.App.Core.Infrastructure.Clients.Events;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Test.Setup;
using ProtoBuf.Meta;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class AltinnEventsClientTests : TestBed<AltinnApiTestFixture>
{
    private readonly IAltinnEventsClient _sut;

    public AltinnEventsClientTests(
        ITestOutputHelper testOutputHelper,
        AltinnApiTestFixture altinnApiTestFixture
    )
        : base(testOutputHelper, altinnApiTestFixture)
    {
        _sut = altinnApiTestFixture.GetService<IAltinnEventsClient>(testOutputHelper)!;
    }

    [Fact]
    public async Task Subscribe_WhenCalledWithValidSubscriptionRequestDto_ReturnsExampleResponse()
    {
        //arrange
        //act
        var result = await _sut.Subscribe(
            new SubscriptionRequest { TypeFilter = "app.instance.process.completed" }
        );
        //assert
        result.ShouldNotBeNull();
        result.TypeFilter.ShouldBe("app.instance.process.completed");
    }
}
