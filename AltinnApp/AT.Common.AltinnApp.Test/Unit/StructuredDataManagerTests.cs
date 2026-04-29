using Altinn.App.Core.Internal.App;
using Altinn.App.Core.Internal.Data;
using Altinn.App.Core.Internal.Instances;
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
    private readonly IInstanceClient _instanceClient;
    private readonly ILogger<StructuredDataManager<TestDataModel, TestStructuredData>> _logger;
    private readonly IStructuredDataValidator<TestStructuredData> _structuredDataValidator;
    private readonly StructuredDataManager<TestDataModel, TestStructuredData> _sut;

    private readonly StructuredDataManager<TestDataModel, TestStructuredData>.Config _config;

    public StructuredDataManagerTests()
    {
        _applicationClient = Substitute.For<IApplicationClient>();
        _instanceClient = Substitute.For<IInstanceClient>();
        _dataClient = Substitute.For<IDataClient>();
        _logger = Substitute.For<
            ILogger<StructuredDataManager<TestDataModel, TestStructuredData>>
        >();
        _structuredDataValidator = Substitute.For<IStructuredDataValidator<TestStructuredData>>();

        _config = new StructuredDataManager<TestDataModel, TestStructuredData>.Config(
            dataModel => new TestStructuredData { Name = dataModel.Name }
        );

        _sut = new StructuredDataManager<TestDataModel, TestStructuredData>(
            _applicationClient,
            _dataClient,
            _instanceClient,
            _config,
            _logger,
            _structuredDataValidator
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
    public async Task End_TaskEnd_UpdatesDataValuesBasedOnConfig()
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
        await _instanceClient
            .Received(1)
            .UpdateDataValues(
                instance,
                Arg.Is<Dictionary<string, string?>>(d =>
                    Enumerable.SequenceEqual(
                        d,
                        new Dictionary<string, string?>()
                        {
                            {
                                StructuredDataManager<
                                    TestDataModel,
                                    TestStructuredData
                                >.StructuredDataTypeIdKey,
                                _config.StructuredDataConfiguration.StructuredDataTypeId
                            },
                            {
                                StructuredDataManager<
                                    TestDataModel,
                                    TestStructuredData
                                >.MainPdfDataTypeId,
                                _config.StructuredDataConfiguration.MainPdfDataTypeId
                            },
                        }
                    )
                )
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
    public async Task End_ProcessEnd_When_KeepAppDataModelAfterMapping_IsTrue_ShouldNotDeleteData()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);

        var sut_withDeleteDisabled = new StructuredDataManager<TestDataModel, TestStructuredData>(
            _applicationClient,
            _dataClient,
            _instanceClient,
            _config with
            {
                StructuredDataConfiguration = _config.StructuredDataConfiguration with
                {
                    KeepAppDataModelAfterMapping = true,
                },
            },
            _logger,
            Substitute.For<IStructuredDataValidator<TestStructuredData>>()
        );

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

    [Fact]
    public async Task End_TaskEnd_DeletesExistingStructuredDataBeforeInserting()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var existingStructuredDataId = Guid.NewGuid();
        instance.Data.Add(
            new DataElement
            {
                Id = existingStructuredDataId.ToString(),
                DataType = "structured-data",
            }
        );

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
            .DeleteData(
                instance.GetInstanceOwnerPartyId(),
                instance.GetInstanceGuid(),
                existingStructuredDataId,
                false,
                cancellationToken: Arg.Any<CancellationToken>()
            );

        Received.InOrder(() =>
        {
            _dataClient.DeleteData(
                instance.GetInstanceOwnerPartyId(),
                instance.GetInstanceGuid(),
                existingStructuredDataId,
                false,
                cancellationToken: Arg.Any<CancellationToken>()
            );
            _dataClient.InsertBinaryData(
                instance.Id,
                "structured-data",
                "application/json",
                "structured-data.json",
                Arg.Any<Stream>(),
                cancellationToken: Arg.Any<CancellationToken>()
            );
        });

        instance.Data.ShouldNotContain(d => d.Id == existingStructuredDataId.ToString());
    }

    [Fact]
    public async Task End_TaskEnd_DeletesAllExistingStructuredDataElements()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        instance.Data.Add(
            new DataElement { Id = firstId.ToString(), DataType = "structured-data" }
        );
        instance.Data.Add(
            new DataElement { Id = secondId.ToString(), DataType = "structured-data" }
        );

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
            .DeleteData(
                instance.GetInstanceOwnerPartyId(),
                instance.GetInstanceGuid(),
                firstId,
                false,
                cancellationToken: Arg.Any<CancellationToken>()
            );
        await _dataClient
            .Received(1)
            .DeleteData(
                instance.GetInstanceOwnerPartyId(),
                instance.GetInstanceGuid(),
                secondId,
                false,
                cancellationToken: Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task End_TaskEnd_DoesNotDeleteWhenNoExistingStructuredData()
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
    public async Task End_TaskEnd_IsIdempotent_WhenCalledTwice()
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

        // Simulate that the first call resulted in a structured-data element being persisted on the instance.
        _dataClient
            .When(c =>
                c.InsertBinaryData(
                    Arg.Any<string>(),
                    "structured-data",
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<Stream>(),
                    cancellationToken: Arg.Any<CancellationToken>()
                )
            )
            .Do(_ =>
            {
                instance.Data.Add(
                    new DataElement { Id = Guid.NewGuid().ToString(), DataType = "structured-data" }
                );
            });

        // Act
        await _sut.End("task1", instance);
        await _sut.End("task1", instance);

        // Assert: only one structured-data element should remain after the second call.
        instance.Data.Count(d => d.DataType == "structured-data").ShouldBe(1);

        // And InsertBinaryData should have been called once per invocation (i.e. twice total).
        await _dataClient
            .Received(2)
            .InsertBinaryData(
                instance.Id,
                "structured-data",
                "application/json",
                "structured-data.json",
                Arg.Any<Stream>(),
                cancellationToken: Arg.Any<CancellationToken>()
            );

        // The second call must have deleted the element added by the first call.
        await _dataClient
            .Received(1)
            .DeleteData(
                instance.GetInstanceOwnerPartyId(),
                instance.GetInstanceGuid(),
                Arg.Any<Guid>(),
                false,
                cancellationToken: Arg.Any<CancellationToken>()
            );
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
