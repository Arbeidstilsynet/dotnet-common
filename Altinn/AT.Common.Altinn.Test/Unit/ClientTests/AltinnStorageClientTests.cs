using Altinn.App.Core.Models;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Test.Unit.Setup;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit.ClientTests;

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

    [Fact]
    public async Task GetInstance_WhenCalledWithValidCloudEvent_ReturnsExampleResponse()
    {
        //arrange
        //act
        var result = await _sut.GetInstance(
            new CloudEvent()
            {
                Source = new Uri(
                    $"https://altinnapp/instances/{DynamicDataGeneration.DefaultIntValue}/{DynamicDataGeneration.DefaultPathUuid}"
                ),
            }
        );
        //assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(DynamicDataGeneration.DefaultPathUuid.ToString());
    }

    [Fact]
    public async Task GetInstances_WhenCalledWithValidQueryParameters_ReturnsExampleResponse()
    {
        //arrange
        //act
        var result = await _sut.GetInstances(
            new InstanceQueryParameters
            {
                AppId = "dat/test",
                Org = "dat",
                ProcessIsComplete = true,
                ExcludeConfirmedBy = "dat",
            }
        );
        //assert
        result.ShouldNotBeNull();
        result.Instances[0].Id.ShouldBe(DynamicDataGeneration.DefaultPathUuid.ToString());
    }
}
