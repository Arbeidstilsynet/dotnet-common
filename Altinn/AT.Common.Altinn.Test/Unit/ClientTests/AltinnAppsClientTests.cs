using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Test.Unit.Setup;
using Shouldly;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit.ClientTests;

public class AltinnAppsClientTests : TestBed<AltinnApiTestFixture>
{
    private readonly IAltinnAppsClient _sut;

    public AltinnAppsClientTests(
        ITestOutputHelper testOutputHelper,
        AltinnApiTestFixture altinnApiTestFixture
    )
        : base(testOutputHelper, altinnApiTestFixture)
    {
        _sut = altinnApiTestFixture.GetService<IAltinnAppsClient>(testOutputHelper)!;
    }

    [Fact]
    public async Task CompleteInstance_WhenCalledWithValidInstanceRequestDto_ReturnsExampleResponse()
    {
        //arrange
        //act
        var result = await _sut.CompleteInstance(
            "testapp",
            new Model.Api.Request.InstanceRequest
            {
                InstanceGuid = DynamicDataGeneration.DefaultPathUuid,
                InstanceOwnerPartyId = DynamicDataGeneration.DefaultIntValue.ToString(),
            }
        );
        //assert
        result.ShouldNotBeNull();
    }
}
