using Altinn.App.Core.Internal.App;
using Altinn.App.Core.Internal.Data;
using Altinn.App.Core.Internal.Instances;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Arbeidstilsynet.Common.AltinnApp.Implementation;
using Arbeidstilsynet.Common.AltinnApp.Model;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Arbeidstilsynet.Common.AltinnApp.Test.Unit.TestFixtures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

#pragma warning disable CS0618 // Verify compatibility with deprecated package-managed cleanup.
public class StructuredDataManagerTests
{
    private readonly IApplicationClient _applicationClient;
    private readonly IDataClient _dataClient;
    private readonly IInstanceClient _instanceClient;
    private readonly ILogger<StructuredDataManager<TestDataModel, TestStructuredData>> _logger;
    private readonly IStructuredDataValidator<TestStructuredData> _structuredDataValidator;
    private readonly StructuredDataManager<TestDataModel, TestStructuredData> _sut;

    private readonly StructuredDataManager<TestDataModel, TestStructuredData>.Config _config;
    private readonly IStructuredDataMapper<TestDataModel, TestStructuredData> _mapper;

    public StructuredDataManagerTests()
    {
        _applicationClient = Substitute.For<IApplicationClient>();
        _instanceClient = Substitute.For<IInstanceClient>();
        _dataClient = Substitute.For<IDataClient>();
        _dataClient
            .InsertBinaryData(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Stream>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(_ => NewStructuredDataElement());
        _dataClient
            .DeleteData(
                Arg.Any<int>(),
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<bool>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(true);
        _logger = Substitute.For<
            ILogger<StructuredDataManager<TestDataModel, TestStructuredData>>
        >();
        _structuredDataValidator = Substitute.For<IStructuredDataValidator<TestStructuredData>>();

        _config = new StructuredDataManager<TestDataModel, TestStructuredData>.Config();
        _mapper = new DelegateStructuredDataMapper<TestDataModel, TestStructuredData>(
            dataModel => new TestStructuredData { Name = dataModel.Name }
        );

        _sut = new StructuredDataManager<TestDataModel, TestStructuredData>(
            _applicationClient,
            _dataClient,
            _instanceClient,
            _config,
            _logger,
            _structuredDataValidator,
            _mapper,
            []
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
    public async Task End_TaskEnd_AwaitsInsertion_ThenProcessEndUsesSameInstanceWithoutRefresh()
    {
        var instance = AltinnData.CreateTestInstance();
        var modelElement = instance.Data.Single();
        var attachment = new DataElement
        {
            Id = Guid.NewGuid().ToString(),
            DataType = "attachment",
        };
        instance.Data.Add(attachment);
        ConfigureMapping();
        var createdElement = NewStructuredDataElement();
        var insertion = new TaskCompletionSource<DataElement>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        _dataClient
            .InsertBinaryData(
                instance.Id,
                "structured-data",
                "application/json",
                "structured-data.json",
                Arg.Any<Stream>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(insertion.Task);

        var mapping = _sut.End("task1", instance);

        mapping.IsCompleted.ShouldBeFalse();
        instance.Data.ShouldBe([modelElement, attachment]);
        await _instanceClient.DidNotReceiveWithAnyArgs().UpdateDataValues(default!, default!);

        insertion.SetResult(createdElement);
        await mapping;
        instance.Data.ShouldBe([modelElement, attachment, createdElement]);

        await _sut.End(instance, []);

        instance.Data.ShouldBe([attachment, createdElement]);
        await _instanceClient.DidNotReceiveWithAnyArgs().GetInstance(default!);
    }

    [Fact]
    public async Task End_TaskEnd_DoesNotDuplicateReturnedElementAlreadyAddedByClient()
    {
        var instance = AltinnData.CreateTestInstance();
        ConfigureMapping();
        var createdElement = NewStructuredDataElement();
        _dataClient
            .InsertBinaryData(
                instance.Id,
                "structured-data",
                "application/json",
                "structured-data.json",
                Arg.Any<Stream>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(_ =>
            {
                instance.Data.Add(
                    new DataElement { Id = createdElement.Id, DataType = createdElement.DataType }
                );
                return createdElement;
            });

        await _sut.End("task1", instance);

        instance.Data.Count(d => d.Id == createdElement.Id).ShouldBe(1);
    }

    [Fact]
    public async Task End_TaskEnd_WhenInsertionFails_RecordsOnlySuccessfulDeletions()
    {
        var instance = AltinnData.CreateTestInstance();
        var modelElement = instance.Data.Single();
        instance.Data.Add(NewStructuredDataElement());
        ConfigureMapping();
        var storageError = new HttpRequestException("Insertion failed");
        _dataClient
            .InsertBinaryData(
                instance.Id,
                "structured-data",
                "application/json",
                "structured-data.json",
                Arg.Any<Stream>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromException<DataElement>(storageError));

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.End("task1", instance)
        );

        exception.InnerException.ShouldBeSameAs(storageError);
        instance.Data.ShouldBe([modelElement]);
        await _instanceClient.DidNotReceiveWithAnyArgs().UpdateDataValues(default!, default!);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task End_TaskEnd_WhenDeletionFails_DoesNotRemoveFailedElementOrInsert(bool throws)
    {
        var instance = AltinnData.CreateTestInstance();
        var modelElement = instance.Data.Single();
        var deletedElement = NewStructuredDataElement();
        var failedElement = NewStructuredDataElement();
        var unattemptedElement = NewStructuredDataElement();
        instance.Data.AddRange([deletedElement, failedElement, unattemptedElement]);
        ConfigureMapping();
        var storageError = new HttpRequestException("Deletion failed");
        _dataClient
            .DeleteData(
                instance.GetInstanceOwnerPartyId(),
                instance.GetInstanceGuid(),
                Guid.Parse(failedElement.Id),
                false,
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(throws ? Task.FromException<bool>(storageError) : Task.FromResult(false));

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.End("task1", instance)
        );

        if (throws)
        {
            exception.InnerException.ShouldBeSameAs(storageError);
        }
        else
        {
            exception
                .InnerException.ShouldBeOfType<InvalidOperationException>()
                .Message.ShouldContain(failedElement.Id);
        }
        instance.Data.ShouldBe([modelElement, failedElement, unattemptedElement]);
        await _dataClient
            .DidNotReceiveWithAnyArgs()
            .InsertBinaryData(
                default!,
                default!,
                default!,
                default!,
                default!,
                cancellationToken: Arg.Any<CancellationToken>()
            );
        await _instanceClient.DidNotReceiveWithAnyArgs().UpdateDataValues(default!, default!);
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
                        d ?? new(),
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
                            {
                                StructuredDataManager<
                                    TestDataModel,
                                    TestStructuredData
                                >.ExcludedAttachmentDataTypeIdsKey,
                                """["testDataType"]"""
                            },
                        }
                    )
                )
            );
    }

    [Fact]
    public async Task End_TaskEnd_AppendsConfiguredAttachmentExclusionsToDatamodelType()
    {
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var sut = new StructuredDataManager<TestDataModel, TestStructuredData>(
            _applicationClient,
            _dataClient,
            _instanceClient,
            _config with
            {
                StructuredDataConfiguration = _config.StructuredDataConfiguration with
                {
                    ExcludedAttachmentDataTypeIds = ["internal-data", "testDataType"],
                },
            },
            _logger,
            _structuredDataValidator,
            _mapper,
            []
        );

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);
        _dataClient
            .GetFormData(
                Arg.Any<Instance>(),
                Arg.Any<DataElement>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(new TestDataModel { Name = "Test" });

        await sut.End("task1", instance);

        await _instanceClient
            .Received(1)
            .UpdateDataValues(
                instance,
                Arg.Is<Dictionary<string, string?>>(dataValues =>
                    dataValues[
                        StructuredDataManager<
                            TestDataModel,
                            TestStructuredData
                        >.ExcludedAttachmentDataTypeIdsKey
                    ] == """["internal-data","testDataType"]"""
                )
            );
    }

    [Theory]
    [InlineData("main-pdf")]
    [InlineData("structured")]
    public async Task End_TaskEnd_WhenDatamodelTypeCollidesWithSignificantType_ShouldThrowAndNotWriteDataValues(
        string collidingWith
    )
    {
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var structuredDataConfiguration =
            collidingWith == "main-pdf"
                ? _config.StructuredDataConfiguration with
                {
                    MainPdfDataTypeId = "TESTDATATYPE",
                }
                : _config.StructuredDataConfiguration with
                {
                    StructuredDataTypeId = "testDataType",
                };
        var sut = new StructuredDataManager<TestDataModel, TestStructuredData>(
            _applicationClient,
            _dataClient,
            _instanceClient,
            _config with
            {
                StructuredDataConfiguration = structuredDataConfiguration,
            },
            _logger,
            _structuredDataValidator,
            _mapper,
            []
        );

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);
        _dataClient
            .GetFormData(
                Arg.Any<Instance>(),
                Arg.Any<DataElement>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(new TestDataModel { Name = "Test" });

        await Should.ThrowAsync<InvalidOperationException>(() => sut.End("task1", instance));

        await _instanceClient.DidNotReceiveWithAnyArgs().UpdateDataValues(default!, default!);
    }

    [Fact]
    public async Task End_ProcessEnd_UsesIApplicationClientToGetDataElement()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        instance.Data.Add(
            new DataElement { Id = Guid.NewGuid().ToString(), DataType = "structured-data" }
        );
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
        instance.Data.Add(
            new DataElement { Id = Guid.NewGuid().ToString(), DataType = "structured-data" }
        );
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
        instance.Data.Add(
            new DataElement { Id = Guid.NewGuid().ToString(), DataType = "structured-data" }
        );
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
            Substitute.For<IStructuredDataValidator<TestStructuredData>>(),
            _mapper,
            []
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
        instance.Data.Add(
            new DataElement { Id = Guid.NewGuid().ToString(), DataType = "structured-data" }
        );
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
    public async Task End_ProcessEnd_WhenNoStructuredDataExists_ShouldThrowAndNotDeleteDataModel()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance(); // only has testDataType, no structured-data
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);

        _instanceClient.GetInstance(instance).Returns(AltinnData.CreateTestInstance());
        var postProcessor = Substitute.For<IStructuredDataPostProcessor>();
        var sut = CreateSut([postProcessor]);

        // Act
        var act = () => sut.End(instance, new List<InstanceEvent>());

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldContain("No structured data element");
        await _instanceClient.Received(1).GetInstance(instance);
        await postProcessor.DidNotReceiveWithAnyArgs().Process(default!);
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
    public async Task End_ProcessEnd_WhenNoStructuredDataExists_And_KeepAppDataModelIsTrue_ShouldThrow()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);

        var sut = new StructuredDataManager<TestDataModel, TestStructuredData>(
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
            _structuredDataValidator,
            _mapper,
            []
        );

        _instanceClient.GetInstance(instance).Returns(AltinnData.CreateTestInstance());

        // Act
        var act = () => sut.End(instance, new List<InstanceEvent>());

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
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
    public async Task End_TaskEnd_DeletesExistingStructuredDataBeforeInserting()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var unrelatedElements = instance.Data.ToList();
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
        instance.Data.Count(d => d.DataType == "structured-data").ShouldBe(1);
        instance.Data.Where(d => d.DataType != "structured-data").ShouldBe(unrelatedElements);
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

    [Fact]
    public async Task End_TaskEnd_WhenTaskIdFilterDoesNotMatch_ShouldSkipStructuredDataGeneration()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var sut = new StructuredDataManager<TestDataModel, TestStructuredData>(
            _applicationClient,
            _dataClient,
            _instanceClient,
            _config with
            {
                StructuredDataConfiguration = _config.StructuredDataConfiguration with
                {
                    TaskIdFilter = ["allowed-task"],
                },
            },
            _logger,
            _structuredDataValidator,
            _mapper,
            []
        );

        // Act
        await sut.End("other-task", instance);

        // Assert
        await _applicationClient.DidNotReceiveWithAnyArgs().GetApplication(default!, default!);
        await _dataClient
            .DidNotReceiveWithAnyArgs()
            .GetFormData(default!, default!, cancellationToken: Arg.Any<CancellationToken>());
        await _dataClient
            .DidNotReceiveWithAnyArgs()
            .InsertBinaryData(
                default!,
                default!,
                default!,
                default!,
                default!,
                cancellationToken: Arg.Any<CancellationToken>()
            );
        await _instanceClient.DidNotReceiveWithAnyArgs().UpdateDataValues(default!, default!);
        await _structuredDataValidator.DidNotReceiveWithAnyArgs().ValidateAndThrow(default!);

        _instanceClient.GetInstance(instance).Returns(AltinnData.CreateTestInstance());
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            sut.End(instance, [])
        );
        exception.Message.ShouldContain("No structured data element");
        await _instanceClient.Received(1).GetInstance(instance);
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
    public async Task End_TaskEnd_WhenTaskIdFilterMatchesIgnoringCase_ShouldGenerateStructuredData()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var dataModel = new TestDataModel { Name = "Test" };
        var sut = new StructuredDataManager<TestDataModel, TestStructuredData>(
            _applicationClient,
            _dataClient,
            _instanceClient,
            _config with
            {
                StructuredDataConfiguration = _config.StructuredDataConfiguration with
                {
                    TaskIdFilter = ["Allowed-Task"],
                },
            },
            _logger,
            _structuredDataValidator,
            _mapper,
            []
        );

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
        await sut.End("allowed-task", instance);

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
        await _instanceClient
            .Received(1)
            .UpdateDataValues(instance, Arg.Any<Dictionary<string, string?>>());
    }

    [Fact]
    public async Task End_ProcessEnd_WithoutPostProcessors_DoesNotFetchInstance()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        instance.Data.Add(
            new DataElement { Id = Guid.NewGuid().ToString(), DataType = "structured-data" }
        );
        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(AltinnData.CreateTestApplication(classRef: typeof(TestDataModel).FullName));

        // Act
        await _sut.End(instance, new List<InstanceEvent>());

        // Assert
        await _instanceClient.DidNotReceiveWithAnyArgs().GetInstance(default(Instance)!);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task End_ProcessEnd_WhenMissingLocally_RefreshesAndReusesFreshState(
        bool keepModel,
        bool hasPostProcessors
    )
    {
        var instance = AltinnData.CreateTestInstance();
        var staleElement = new DataElement { Id = Guid.NewGuid().ToString(), DataType = "stale" };
        instance.Data.Add(staleElement);
        var freshInstance = CreateInstanceWithRequiredElements();
        if (keepModel)
        {
            // Altinn's automatic deletion can remove the model before IProcessEnd.
            freshInstance.Data.RemoveAll(d => d.DataType == "testDataType");
        }
        freshInstance.Data.Single(d => d.DataType == "structured-data").DataType =
            "STRUCTURED-DATA";
        _instanceClient.GetInstance(instance).Returns(freshInstance);
        ConfigureMapping();
        var postProcessor = Substitute.For<IStructuredDataPostProcessor>();
        var sut = CreateSut(
            hasPostProcessors ? [postProcessor] : [],
            _config with
            {
                StructuredDataConfiguration = _config.StructuredDataConfiguration with
                {
                    KeepAppDataModelAfterMapping = keepModel,
                },
            }
        );

        await sut.End(instance, []);

        await _instanceClient.Received(1).GetInstance(instance);
        instance.Data.ShouldBeSameAs(freshInstance.Data);
        instance.Data.ShouldNotContain(staleElement);
        instance.Data.ShouldNotContain(d => d.DataType == "testDataType");
        if (hasPostProcessors)
        {
            await postProcessor
                .Received(1)
                .Process(
                    Arg.Is<StructuredDataPostProcessingContext>(c =>
                        c.Instance == freshInstance
                        && c.StructuredDataElement
                            == freshInstance.Data.Single(d => d.DataType == "STRUCTURED-DATA")
                        && c.MainPdfElement
                            == freshInstance.Data.Single(d => d.DataType == "ref-data-as-pdf")
                    )
                );
        }
        if (keepModel)
        {
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
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task End_ProcessEnd_WhenRefreshFails_PropagatesStorageErrorWithoutDeleting(
        bool structuredDataPresentLocally
    )
    {
        var instance = AltinnData.CreateTestInstance();
        if (structuredDataPresentLocally)
        {
            instance.Data.Add(NewStructuredDataElement());
        }
        var originalData = instance.Data.ToList();
        var storageError = new HttpRequestException("Storage unavailable");
        _instanceClient.GetInstance(instance).Returns(Task.FromException<Instance>(storageError));
        var postProcessor = Substitute.For<IStructuredDataPostProcessor>();
        var sut = CreateSut([postProcessor]);

        var exception = await Should.ThrowAsync<HttpRequestException>(() => sut.End(instance, []));

        exception.ShouldBeSameAs(storageError);
        instance.Data.ShouldBe(originalData);
        await _instanceClient.Received(1).GetInstance(instance);
        await postProcessor.DidNotReceiveWithAnyArgs().Process(default!);
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task End_ProcessEnd_WhenManualDeletionFails_PreservesModel(bool throws)
    {
        var instance = CreateInstanceWithRequiredElements();
        var originalData = instance.Data.ToList();
        ConfigureMapping();
        var storageError = new HttpRequestException("Deletion failed");
        _dataClient
            .DeleteData(
                instance.GetInstanceOwnerPartyId(),
                instance.GetInstanceGuid(),
                Guid.Parse(instance.Data.Single(d => d.DataType == "testDataType").Id),
                false,
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(throws ? Task.FromException<bool>(storageError) : Task.FromResult(false));

        var exception = await Should.ThrowAsync<Exception>(() => _sut.End(instance, []));

        if (throws)
        {
            exception.ShouldBeSameAs(storageError);
        }
        else
        {
            exception
                .ShouldBeOfType<InvalidOperationException>()
                .Message.ShouldContain("Could not delete");
        }
        instance.Data.ShouldBe(originalData);
    }

    [Fact]
    public async Task End_ProcessEnd_RunsPostProcessorsInOrder_WithFreshInstanceAndLatestElements()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        instance.Data.Add(
            new DataElement { Id = Guid.NewGuid().ToString(), DataType = "structured-data" }
        );
        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(AltinnData.CreateTestApplication(classRef: typeof(TestDataModel).FullName));

        var structuredDataElement = new DataElement
        {
            Id = Guid.NewGuid().ToString(),
            DataType = "structured-data",
        };
        var olderPdfElement = new DataElement
        {
            Id = Guid.NewGuid().ToString(),
            DataType = "ref-data-as-pdf",
            Created = DateTime.UtcNow.AddMinutes(-5),
        };
        var latestPdfElement = new DataElement
        {
            Id = Guid.NewGuid().ToString(),
            DataType = "ref-data-as-pdf",
            Created = DateTime.UtcNow,
        };
        var freshInstance = AltinnData.CreateTestInstance();
        freshInstance.Data.AddRange([structuredDataElement, olderPdfElement, latestPdfElement]);
        _instanceClient.GetInstance(instance).Returns(freshInstance);

        var first = Substitute.For<IStructuredDataPostProcessor>();
        var second = Substitute.For<IStructuredDataPostProcessor>();
        var sut = CreateSut(postProcessors: [first, second]);

        // Act
        await sut.End(instance, new List<InstanceEvent>());

        // Assert
        Received.InOrder(() =>
        {
            first.Process(Arg.Any<StructuredDataPostProcessingContext>());
            second.Process(Arg.Any<StructuredDataPostProcessingContext>());
        });
        await first
            .Received(1)
            .Process(
                Arg.Is<StructuredDataPostProcessingContext>(c =>
                    c.Instance == freshInstance
                    && c.StructuredDataElement == structuredDataElement
                    && c.MainPdfElement == latestPdfElement
                )
            );
    }

    [Fact]
    public async Task End_ProcessEnd_RunsPostProcessorsBeforeDeletingDataModel()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        instance.Data.Single().Id = Guid.NewGuid().ToString();
        instance.Data.Add(
            new DataElement { Id = Guid.NewGuid().ToString(), DataType = "structured-data" }
        );
        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(AltinnData.CreateTestApplication(classRef: typeof(TestDataModel).FullName));
        var freshInstance = CreateInstanceWithRequiredElements();
        _instanceClient.GetInstance(instance).Returns(freshInstance);

        var postProcessor = Substitute.For<IStructuredDataPostProcessor>();
        var sut = CreateSut(postProcessors: [postProcessor]);

        // Act
        await sut.End(instance, new List<InstanceEvent>());

        // Assert
        Received.InOrder(() =>
        {
            _instanceClient.GetInstance(instance);
            postProcessor.Process(Arg.Any<StructuredDataPostProcessingContext>());
            _dataClient.DeleteData(
                Arg.Any<int>(),
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                false,
                cancellationToken: Arg.Any<CancellationToken>()
            );
        });
        await _dataClient
            .Received(1)
            .DeleteData(
                instance.GetInstanceOwnerPartyId(),
                instance.GetInstanceGuid(),
                Guid.Parse("12345678-1234-1234-1234-123456789012"),
                false,
                cancellationToken: Arg.Any<CancellationToken>()
            );
        instance.Data.ShouldBeSameAs(freshInstance.Data);
        instance.Data.ShouldNotContain(d => d.DataType == "testDataType");
    }

    [Fact]
    public async Task End_ProcessEnd_WhenPostProcessorThrows_ShouldNotDeleteDataModel()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        instance.Data.Add(
            new DataElement { Id = Guid.NewGuid().ToString(), DataType = "structured-data" }
        );
        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(AltinnData.CreateTestApplication(classRef: typeof(TestDataModel).FullName));
        _instanceClient.GetInstance(instance).Returns(CreateInstanceWithRequiredElements());

        var postProcessor = Substitute.For<IStructuredDataPostProcessor>();
        postProcessor
            .Process(Arg.Any<StructuredDataPostProcessingContext>())
            .Returns(Task.FromException(new InvalidOperationException("boom")));
        var sut = CreateSut(postProcessors: [postProcessor]);
        // Act
        var act = () => sut.End(instance, new List<InstanceEvent>());

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
        await _dataClient
            .DidNotReceiveWithAnyArgs()
            .DeleteData(
                default,
                default,
                default,
                default,
                cancellationToken: Arg.Any<CancellationToken>()
            );
        instance.Data.ShouldContain(d => d.DataType == "testDataType");
    }

    [Theory]
    [InlineData("structured-data")]
    [InlineData("ref-data-as-pdf")]
    public async Task End_ProcessEnd_WhenRequiredElementIsMissingOnFreshInstance_ShouldThrowAndNotRunPostProcessors(
        string missingDataType
    )
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        instance.Data.Add(
            new DataElement { Id = Guid.NewGuid().ToString(), DataType = "structured-data" }
        );
        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(AltinnData.CreateTestApplication(classRef: typeof(TestDataModel).FullName));

        var freshInstance = CreateInstanceWithRequiredElements();
        freshInstance.Data.RemoveAll(d => d.DataType == missingDataType);
        _instanceClient.GetInstance(instance).Returns(freshInstance);

        var postProcessor = Substitute.For<IStructuredDataPostProcessor>();
        var sut = CreateSut(postProcessors: [postProcessor]);

        // Act
        var act = () => sut.End(instance, new List<InstanceEvent>());

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldContain(missingDataType);
        await postProcessor.DidNotReceiveWithAnyArgs().Process(default!);
    }

    [Fact]
    public async Task End_TaskEnd_DoesNotRunPostProcessors()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(AltinnData.CreateTestApplication(classRef: typeof(TestDataModel).FullName));
        _dataClient
            .GetFormData(
                Arg.Any<Instance>(),
                Arg.Any<DataElement>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(new TestDataModel { Name = "Test" });

        var postProcessor = Substitute.For<IStructuredDataPostProcessor>();
        var sut = CreateSut(postProcessors: [postProcessor]);

        // Act
        await sut.End("task1", instance);

        // Assert
        await postProcessor.DidNotReceiveWithAnyArgs().Process(default!);
    }

    private void ConfigureMapping()
    {
        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(AltinnData.CreateTestApplication(classRef: typeof(TestDataModel).FullName));
        _dataClient
            .GetFormData(
                Arg.Any<Instance>(),
                Arg.Any<DataElement>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(new TestDataModel { Name = "Test" });
    }

    private StructuredDataManager<TestDataModel, TestStructuredData> CreateSut(
        IEnumerable<IStructuredDataPostProcessor> postProcessors,
        StructuredDataManager<TestDataModel, TestStructuredData>.Config? config = null
    ) =>
        new(
            _applicationClient,
            _dataClient,
            _instanceClient,
            config ?? _config,
            _logger,
            _structuredDataValidator,
            _mapper,
            postProcessors
        );

    private static Instance CreateInstanceWithRequiredElements()
    {
        var instance = AltinnData.CreateTestInstance();
        instance.Data.AddRange([
            new DataElement { Id = Guid.NewGuid().ToString(), DataType = "structured-data" },
            new DataElement { Id = Guid.NewGuid().ToString(), DataType = "ref-data-as-pdf" },
        ]);
        return instance;
    }

    private static DataElement NewStructuredDataElement() =>
        new() { Id = Guid.NewGuid().ToString(), DataType = "structured-data" };

    public class TestDataModel
    {
        public string? Name { get; set; }
    }
#pragma warning restore CS0618

    public class TestStructuredData
    {
        public string? Name { get; set; }
    }
}
