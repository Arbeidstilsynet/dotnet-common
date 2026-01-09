using Altinn.App.Core.Internal.Data;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Arbeidstilsynet.Common.AltinnApp.Test.Unit.TestFixtures;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class DataClientExtensionsTests
{
    private readonly Instance _instance;
    private readonly IDataClient _sut = Substitute.For<IDataClient>();

    public DataClientExtensionsTests()
    {
        _instance = AltinnData.CreateTestInstance(
            org: "org",
            appId: "dat/appId",
            partyId: "1337",
            instanceId: $"dat/{Guid.NewGuid()}"
        );
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
            .GetFormData(_instance, dataElement, cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSkjemaData_WhenThereAreNoDataElementsOfGivenDataType_ReturnsDefault()
    {
        Guid.NewGuid();

        // Arrange
        Substitute.For<DataElement>();
        _instance.Data = [];

        var result = await _sut.GetSkjemaData<object>(_instance);

        await _sut.DidNotReceiveWithAnyArgs()
            .GetFormData(
                default!,
                default!,
                cancellationToken: TestContext.Current.CancellationToken
            );

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
                _instance.GetInstanceOwnerPartyId(),
                _instance.GetInstanceGuid(),
                elementToDeleteGuid,
                false,
                cancellationToken: Arg.Any<CancellationToken>()
            );

        _instance.Data.Count.ShouldBe(1);
        _instance.Data.ShouldContain(otherDataElement);
    }
}
