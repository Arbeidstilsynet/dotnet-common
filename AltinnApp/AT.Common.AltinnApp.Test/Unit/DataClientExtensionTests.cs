using Altinn.App.Core.Internal.Data;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class DataClientExtensionsTests
{
    private readonly Instance _instance = Substitute.For<Instance>();
    private readonly IDataClient _sut = Substitute.For<IDataClient>();

    public DataClientExtensionsTests()
    {
        _instance.Org = "org";
        _instance.Id = $"dat/{Guid.NewGuid()}";
        _instance.AppId = "dat/appId";
        _instance.InstanceOwner = new InstanceOwner { PartyId = "1337" };
    }

    [Fact]
    public async Task GetSkjemaData_CallsGetFormDataForDataType_structured_data_ByDefault()
    {
        var elementGuid = Guid.NewGuid();

        // Arrange
        var dataElement = Substitute.For<DataElement>();
        dataElement.DataType = "structured-data";
        dataElement.Id = elementGuid.ToString();
        _instance.Data = [dataElement];

        _ = await _sut.GetSkjemaData<object>(_instance);

        await _sut.Received(1)
            .GetFormData(
                _instance.GetInstanceGuid(),
                typeof(object),
                _instance.Org!,
                _instance.GetAppName(),
                _instance.GetInstanceOwnerPartyId(),
                elementGuid
            );
    }

    [Fact]
    public async Task GetSkjemaData_WhenThereAreNoDataElementsOfGivenDataType_ReturnsDefault()
    {
        var elementGuid = Guid.NewGuid();

        // Arrange
        var dataElement = Substitute.For<DataElement>();
        _instance.Data = [];

        var result = await _sut.GetSkjemaData<object>(_instance);

        await _sut.DidNotReceiveWithAnyArgs()
            .GetFormData(default!, typeof(object), default!, default!, default!, default!);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteElement_CallsDeleteDataWithCorrectParameters()
    {
        var elementToDeleteGuid = Guid.NewGuid();

        // Arrange
        var elementToDelete = new DataElement { Id = elementToDeleteGuid.ToString() };
        var otherDataElement = new DataElement { Id = Guid.NewGuid().ToString() };

        _instance.Data = [elementToDelete, otherDataElement];

        await _sut.DeleteElement(_instance, elementToDelete);

        await _sut.Received(1)
            .DeleteData(
                _instance.Org!,
                _instance.GetAppName(),
                _instance.GetInstanceOwnerPartyId(),
                _instance.GetInstanceGuid(),
                elementToDeleteGuid,
                false
            );

        _instance.Data.Count.ShouldBe(1);
        _instance.Data.ShouldContain(otherDataElement);
    }
}
