using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Test.Unit.Setup;
using Shouldly;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit.ClientTests;

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
            new AltinnSubscriptionRequest { TypeFilter = "app.instance.process.completed" }
        );
        //assert
        result.ShouldNotBeNull();
        result.TypeFilter.ShouldBe("app.instance.process.completed");
    }

    [Fact]
    public async Task Unsubscribe_WhenCalledWithValidId_ReturnsExampleResponse()
    {
        //arrange
        //act
        var result = await _sut.Unsubscribe(42);
        //assert
        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAltinnSubscription_WhenCalledWithValidId_ReturnsExampleResponse()
    {
        //arrange
        //act
        var result = await _sut.GetAltinnSubscription(42);
        //assert
        result.ShouldNotBeNull();
        result.TypeFilter.ShouldBe("app.instance.process.completed");
    }
}
