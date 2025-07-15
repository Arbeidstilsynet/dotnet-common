using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.Altinn.Test.Setup;
using ProtoBuf.Meta;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class AltinnStorageClientTests : TestBed<AltinnApiTestFixture>
{
    private readonly IAltinnStorageClient _sut;

    public AltinnStorageClientTests(
        ITestOutputHelper testOutputHelper,
        AltinnApiTestFixture altinnApiTestFixture
    )
        : base(testOutputHelper, altinnApiTestFixture)
    {
        _sut = altinnApiTestFixture.GetService<IAltinnStorageClient>(testOutputHelper)!;
    }

    [Fact]
    public async Task GetInstance_WhenCalledWithValidInstanceRequestDto_ReturnsExampleResponse()
    {
        //arrange
        //act
        var result = await _sut.GetInstance(
            new Model.Api.Request.InstanceRequest
            {
                InstanceGuid = DynamicDataGeneration.DefaultPathUuid,
                InstanceOwnerPartyId = DynamicDataGeneration.DefaultIntValue.ToString(),
            }
        );
        //assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(DynamicDataGeneration.DefaultPathUuid.ToString());
    }

    [Fact]
    public async Task CompleteInstance_WhenCalledWithValidInstanceRequestDto_ReturnsExampleResponse()
    {
        //arrange
        //act
        var result = await _sut.CompleteInstance(
            new Model.Api.Request.InstanceRequest
            {
                InstanceGuid = DynamicDataGeneration.DefaultPathUuid,
                InstanceOwnerPartyId = DynamicDataGeneration.DefaultIntValue.ToString(),
            }
        );
        //assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetInstanceData_WhenCalledWithValidInstanceDataRequestDto_ReturnsExampleResponse()
    {
        //arrange
        //act
        var result = await _sut.GetInstanceData(
            new Model.Api.Request.InstanceDataRequest
            {
                DataId = DynamicDataGeneration.DefaultPathUuid,
                InstanceRequest = new Model.Api.Request.InstanceRequest
                {
                    InstanceGuid = DynamicDataGeneration.DefaultPathUuid,
                    InstanceOwnerPartyId = DynamicDataGeneration.DefaultIntValue.ToString(),
                },
            }
        );
        //assert
        result.ShouldNotBeNull();
    }
}
