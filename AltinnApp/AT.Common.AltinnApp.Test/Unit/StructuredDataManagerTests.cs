using Altinn.App.Core.Helpers.DataModel;
using Altinn.App.Core.Internal.App;
using Altinn.App.Core.Internal.Data;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Arbeidstilsynet.Common.AltinnApp.Implementation;
using Arbeidstilsynet.Common.AltinnApp.Test.Unit.TestFixtures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class StructuredDataManagerTests
{
    private readonly IApplicationClient _applicationClient;
    private readonly IDataClient _dataClient;
    private readonly ILogger<StructuredDataManager<TestDataModel, TestStructuredData>> _logger;
    private readonly StructuredDataManager<TestDataModel, TestStructuredData> _sut;

    private readonly StructuredDataManager<TestDataModel, TestStructuredData>.Config _config;

    public StructuredDataManagerTests()
    {
        _applicationClient = Substitute.For<IApplicationClient>();
        _dataClient = Substitute.For<IDataClient>();
        _logger = Substitute.For<
            ILogger<StructuredDataManager<TestDataModel, TestStructuredData>>
        >();

        _config = new StructuredDataManager<TestDataModel, TestStructuredData>.Config(
            dataModel => new TestStructuredData { Name = dataModel.Name }
        );

        _sut = new StructuredDataManager<TestDataModel, TestStructuredData>(
            _applicationClient,
            _dataClient,
            _config,
            _logger
        );
    }

    [Fact]
    public async Task End_TaskEnd_UsesIApplicationClientCorrectly()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var dataModel = new TestDataModel { Name = "Test" };

        _applicationClient.GetApplication(instance.Org, "testApp").Returns(application);
        _dataClient
            .GetFormData(
                Arg.Any<Instance>(),
                Arg.Any<DataElement>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(dataModel);

        // Act
        await _sut.End("task1", instance);

        // Assert
        await _applicationClient.Received(1).GetApplication(instance.Org, "testApp");
    }

    [Fact]
    public async Task End_TaskEnd_UsesIDataClientToGetFormData()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var dataModel = new TestDataModel { Name = "Test" };
        var expectedGuid = Guid.Parse(instance.Data.First().Id);

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);
        _dataClient
            .GetFormData(
                Arg.Any<Instance>(),
                Arg.Any<DataElement>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(dataModel);

        // Act
        await _sut.End("task1", instance);

        // Assert
        await _dataClient
            .Received(1)
            .GetFormData(
                instance,
                Arg.Any<DataElement>(),
                cancellationToken: Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task End_TaskEnd_UsesIDataClientToInsertStructuredData()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var dataModel = new TestDataModel { Name = "Test" };

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);
        _dataClient
            .GetFormData(
                Arg.Any<Instance>(),
                Arg.Any<DataElement>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(dataModel);

        // Act
        await _sut.End("task1", instance);

        // Assert
        await _dataClient
            .Received(1)
            .InsertBinaryData(
                instance.Id,
                "structured-data",
                "application/json",
                "structured-data.json",
                Arg.Any<Stream>(),
                cancellationToken: Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task End_ProcessEnd_UsesIApplicationClientToGetDataElement()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );

        _applicationClient.GetApplication(instance.Org, "testApp").Returns(application);

        // Act
        await _sut.End(instance, new List<InstanceEvent>());

        // Assert
        await _applicationClient.Received(1).GetApplication(instance.Org, "testApp");
    }

    [Fact]
    public async Task End_ProcessEnd_UsesIDataClientToDeleteData()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var dataElement = instance.Data.First();
        var expectedGuid = Guid.Parse(dataElement.Id);

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);

        // Act
        await _sut.End(instance, new List<InstanceEvent>());

        // Assert
        await _dataClient
            .Received(1)
            .DeleteData(
                instance.GetInstanceOwnerPartyId(),
                instance.GetInstanceGuid(),
                expectedGuid,
                false,
                cancellationToken: Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task End_ProcessEnd_When_DeleteAppDataModelAfterMapping_IsFalse_ShouldNotDeleteData()
    {
        // Arrange
        var instance = CreateTestInstance();
        var application = CreateTestApplication();

        var sut_withDeleteDisabled = new StructuredDataManager<TestDataModel, TestStructuredData>(
            _applicationClient,
            _dataClient,
            _config with
            {
                DeleteAppDataModelAfterMapping = false,
            },
            _logger
        );

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);

        // Act
        await sut_withDeleteDisabled.End(instance, new List<InstanceEvent>());

        // Assert
        await _dataClient
            .DidNotReceiveWithAnyArgs()
            .DeleteData(
                default,
                default,
                default,
                default,
                cancellationToken: Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task End_ProcessEnd_RemovesDataElementFromInstance()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var initialDataCount = instance.Data.Count;

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);

        // Act
        await _sut.End(instance, new List<InstanceEvent>());

        // Assert
        instance.Data.Count.ShouldBe(initialDataCount - 1);
    }

    public class TestDataModel
    {
        public string? Name { get; set; }
    }

    public class TestStructuredData
    {
        public string? Name { get; set; }
    }
}
