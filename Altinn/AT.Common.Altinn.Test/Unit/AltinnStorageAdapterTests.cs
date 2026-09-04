using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class AltinnStorageAdapterTests
{
    private readonly IAltinnStorageClient _client = Substitute.For<IAltinnStorageClient>();
    private readonly AltinnStorageAdapter _sut;

    public AltinnStorageAdapterTests()
    {
        _sut = new AltinnStorageAdapter(_client);
    }

    [Fact]
    public async Task GetInstance_DelegatesToStorageClient()
    {
        var instanceId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var expected = new AltinnInstance { Id = $"123/{instanceId}" };
        _client.GetInstance(instanceId, cancellation.Token).Returns(expected);

        var result = await _sut.GetInstance(instanceId, cancellation.Token);

        result.ShouldBeSameAs(expected);
        await _client.Received(1).GetInstance(instanceId, cancellation.Token);
    }

    [Fact]
    public async Task GetDataElements_ReturnsTheInstancesDataElements()
    {
        var instanceId = Guid.NewGuid();
        List<DataElement> expected =
        [
            new() { Id = Guid.NewGuid().ToString(), DataType = "model" },
            new() { Id = Guid.NewGuid().ToString(), DataType = "attachment" },
        ];
        _client.GetInstance(instanceId).Returns(new AltinnInstance { Data = expected });

        var result = await _sut.GetDataElements(instanceId);

        result.ShouldBeSameAs(expected);
    }

    [Fact]
    public async Task GetDataElements_WhenTheInstanceHasNoData_ReturnsNull()
    {
        var instanceId = Guid.NewGuid();
        _client.GetInstance(instanceId).Returns(new AltinnInstance { Data = null });

        var result = await _sut.GetDataElements(instanceId);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetDataElement_ReturnsTheMatchingDataElement()
    {
        var instanceId = Guid.NewGuid();
        var dataElementId = Guid.NewGuid();
        var expected = new DataElement { Id = dataElementId.ToString(), DataType = "attachment" };
        _client
            .GetInstance(instanceId)
            .Returns(
                new AltinnInstance
                {
                    Data = [new DataElement { Id = Guid.NewGuid().ToString() }, expected],
                }
            );

        var result = await _sut.GetDataElement(instanceId, dataElementId);

        result.ShouldBeSameAs(expected);
    }

    [Fact]
    public async Task GetDataElement_WhenThereIsNoMatch_ReturnsNull()
    {
        var instanceId = Guid.NewGuid();
        _client
            .GetInstance(instanceId)
            .Returns(
                new AltinnInstance { Data = [new DataElement { Id = Guid.NewGuid().ToString() }] }
            );

        var result = await _sut.GetDataElement(instanceId, Guid.NewGuid());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetDataElementContent_ConstructsTheStorageRequest()
    {
        var instanceId = Guid.NewGuid();
        var dataElementId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var expected = new MemoryStream([1, 2, 3]);
        _client
            .GetInstance(instanceId, cancellation.Token)
            .Returns(
                new AltinnInstance { InstanceOwner = new InstanceOwner { PartyId = "123456" } }
            );
        _client
            .GetInstanceData(
                Arg.Is<InstanceDataRequest>(request => Matches(request, instanceId, dataElementId)),
                cancellation.Token
            )
            .Returns(expected);

        var result = await _sut.GetDataElementContent(
            instanceId,
            dataElementId,
            cancellation.Token
        );

        result.ShouldBeSameAs(expected);
        await _client
            .Received(1)
            .GetInstanceData(
                Arg.Is<InstanceDataRequest>(request => Matches(request, instanceId, dataElementId)),
                cancellation.Token
            );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetDataElementContent_WithoutAnOwnerPartyId_ReturnsNull(string? partyId)
    {
        var instanceId = Guid.NewGuid();
        _client
            .GetInstance(instanceId)
            .Returns(
                new AltinnInstance
                {
                    InstanceOwner = partyId is null
                        ? null
                        : new InstanceOwner { PartyId = partyId },
                }
            );

        var result = await _sut.GetDataElementContent(instanceId, Guid.NewGuid());

        result.ShouldBeNull();
        await _client
            .DidNotReceive()
            .GetInstanceData(Arg.Any<InstanceDataRequest>(), Arg.Any<CancellationToken>());
    }

    private static bool Matches(InstanceDataRequest? request, Guid instanceId, Guid dataElementId)
    {
        return request
                is {
                    DataId: var requestDataId,
                    InstanceRequest:
                    { InstanceGuid: var requestInstanceId, InstanceOwnerPartyId: "123456" },
                }
            && requestDataId == dataElementId
            && requestInstanceId == instanceId;
    }
}
