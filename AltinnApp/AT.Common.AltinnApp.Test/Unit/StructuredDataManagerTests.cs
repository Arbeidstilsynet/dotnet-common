using Altinn.App.Core.Internal.App;
using Altinn.App.Core.Internal.Data;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Arbeidstilsynet.Common.AltinnApp.Implementation;
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

    public StructuredDataManagerTests()
    {
        _applicationClient = Substitute.For<IApplicationClient>();
        _dataClient = Substitute.For<IDataClient>();
        _logger = Substitute.For<ILogger<StructuredDataManager<TestDataModel, TestStructuredData>>>();
        
        var config = new StructuredDataManager<TestDataModel, TestStructuredData>.Config(
            dataModel => new TestStructuredData { Name = dataModel.Name });
        
        _sut = new StructuredDataManager<TestDataModel, TestStructuredData>(
            _applicationClient,
            _dataClient,
            config,
            _logger);
    }

    [Fact]
    public async Task End_TaskEnd_UsesIApplicationClientCorrectly()
    {
        // Arrange
        var instance = CreateTestInstance();
        var application = CreateTestApplication();
        var dataModel = new TestDataModel { Name = "Test" };

        _applicationClient.GetApplication(instance.Org, "testApp").Returns(application);
        _dataClient.GetFormData(Arg.Any<Guid>(), typeof(TestDataModel), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<Guid>()).Returns(dataModel);

        // Act
        await _sut.End("task1", instance);

        // Assert
        await _applicationClient.Received(1).GetApplication(instance.Org, "testApp");
    }

    [Fact]
    public async Task End_TaskEnd_UsesIDataClientToGetFormData()
    {
        // Arrange
        var instance = CreateTestInstance();
        var application = CreateTestApplication();
        var dataModel = new TestDataModel { Name = "Test" };
        var expectedGuid = Guid.Parse(instance.Data.First().Id);

        _applicationClient.GetApplication(Arg.Any<string>(), Arg.Any<string>()).Returns(application);
        _dataClient.GetFormData(Arg.Any<Guid>(), typeof(TestDataModel), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<Guid>()).Returns(dataModel);

        // Act
        await _sut.End("task1", instance);

        // Assert
        await _dataClient.Received(1).GetFormData(
            instance.GetInstanceGuid(),
            typeof(TestDataModel),
            instance.Org,
            instance.AppId,
            instance.GetInstanceOwnerPartyId(),
            expectedGuid);
    }

    [Fact]
    public async Task End_TaskEnd_UsesIDataClientToInsertStructuredData()
    {
        // Arrange
        var instance = CreateTestInstance();
        var application = CreateTestApplication();
        var dataModel = new TestDataModel { Name = "Test" };

        _applicationClient.GetApplication(Arg.Any<string>(), Arg.Any<string>()).Returns(application);
        _dataClient.GetFormData(Arg.Any<Guid>(), Arg.Any<Type>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<Guid>()).Returns(dataModel);

        // Act
        await _sut.End("task1", instance);

        // Assert
        await _dataClient.Received(1).InsertBinaryData(
            instance.Id,
            "structured-data",
            "application/json",
            "structured-data.json",
            Arg.Any<Stream>());
    }

    [Fact]
    public async Task End_ProcessEnd_UsesIApplicationClientToGetDataElement()
    {
        // Arrange
        var instance = CreateTestInstance();
        var application = CreateTestApplication();

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
        var instance = CreateTestInstance();
        var application = CreateTestApplication();
        var dataElement = instance.Data.First();
        var expectedGuid = Guid.Parse(dataElement.Id);

        _applicationClient.GetApplication(Arg.Any<string>(), Arg.Any<string>()).Returns(application);

        // Act
        await _sut.End(instance, new List<InstanceEvent>());

        // Assert
        await _dataClient.Received(1).DeleteData(
            instance.Org,
            "testApp",
            instance.GetInstanceOwnerPartyId(),
            instance.GetInstanceGuid(),
            expectedGuid,
            false);
    }

    [Fact]
    public async Task End_ProcessEnd_RemovesDataElementFromInstance()
    {
        // Arrange
        var instance = CreateTestInstance();
        var application = CreateTestApplication();
        var initialDataCount = instance.Data.Count;

        _applicationClient.GetApplication(Arg.Any<string>(), Arg.Any<string>()).Returns(application);

        // Act
        await _sut.End(instance, new List<InstanceEvent>());

        // Assert
        instance.Data.Count.ShouldBe(initialDataCount-1);
    }

    private static Instance CreateTestInstance()
    {
        return new Instance
        {
            Id = "50001234/fa0678ad-960d-4307-aba2-ba29c9804c9d",
            Org = "testOrg",
            AppId = "testOrg/testApp",
            InstanceOwner = new InstanceOwner { PartyId = "50001234" },
            Data = new List<DataElement>
            {
                new()
                {
                    Id = "12345678-1234-1234-1234-123456789012",
                    DataType = "testDataType"
                }
            }
        };
    }

    private static Application CreateTestApplication()
    {
        return new Application
        {
            DataTypes = new List<DataType>
            {
                new()
                {
                    Id = "testDataType",
                    AppLogic = new ApplicationLogic()
                    {
                        ClassRef = typeof(TestDataModel).FullName
                    }
                }
            }
        };
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