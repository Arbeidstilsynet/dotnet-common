using Altinn.App.Core.Internal.App;
using Altinn.App.Core.Internal.Data;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Abstract.Processing;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Arbeidstilsynet.Common.AltinnApp.Test.Unit.TestFixtures;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class PreSubmitProcessorTests
{
    private readonly IDataClient _dataClient;
    private readonly IApplicationClient _applicationClient;
    private readonly TestPreSubmitProcessor _sut;

    public PreSubmitProcessorTests()
    {
        _dataClient = Substitute.For<IDataClient>();
        _applicationClient = Substitute.For<IApplicationClient>();
        _sut = new TestPreSubmitProcessor(_dataClient, _applicationClient);
    }

    [Fact]
    public async Task End_UsesIApplicationClientToGetDataElement()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var dataModel = new TestDataModel { Value = "Test" };

        _applicationClient.GetApplication(instance.Org, Arg.Any<string>()).Returns(application);

        _dataClient
            .GetFormData(
                Arg.Any<Guid>(),
                typeof(TestDataModel),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<Guid>(),
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Returns(dataModel);

        // Act
        await _sut.End("task1", instance);

        // Assert
        await _applicationClient.Received(1).GetApplication(instance.Org, "testApp");
    }

    [Fact]
    public async Task End_UsesIDataClientToGetFormData()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var dataElement = instance.Data.First();
        var dataModel = new TestDataModel { Value = "Test" };
        var expectedGuid = Guid.Parse(dataElement.Id);

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);

        _dataClient
            .GetFormData(
                Arg.Any<Guid>(),
                typeof(TestDataModel),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<Guid>(),
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Returns(dataModel);

        // Act
        await _sut.End("task1", instance);

        // Assert
        await _dataClient
            .Received(1)
            .GetFormData(
                instance.GetInstanceGuid(),
                typeof(TestDataModel),
                instance.Org,
                instance.AppId,
                instance.GetInstanceOwnerPartyId(),
                expectedGuid,
                cancellationToken: TestContext.Current.CancellationToken
            );
    }

    [Fact]
    public async Task End_CallsProcessDataModelWithCorrectParameters()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var dataModel = new TestDataModel { Value = "Original" };

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);

        _dataClient
            .GetFormData(
                Arg.Any<Guid>(),
                Arg.Any<Type>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<Guid>(),
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Returns(dataModel);

        // Act
        await _sut.End("task1", instance);

        // Assert
        _sut.LastProcessedDataModel.ShouldBe(dataModel);
        _sut.LastProcessedInstance.ShouldBe(instance);
    }

    [Fact]
    public async Task End_UsesIDataClientToUpdateDataElement()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var dataModel = new TestDataModel { Value = "Original" };

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);

        _dataClient
            .GetFormData(
                Arg.Any<Guid>(),
                Arg.Any<Type>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<Guid>(),
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Returns(dataModel);

        // Act
        await _sut.End("task1", instance);

        // Assert
        await _dataClient
            .Received(1)
            .UpdateData(
                Arg.Is<TestDataModel>(d => d.Value == "Processed"),
                Arg.Any<Guid>(),
                typeof(TestDataModel),
                instance.Org,
                instance.AppId,
                instance.GetInstanceOwnerPartyId(),
                Arg.Any<Guid>(),
                cancellationToken: TestContext.Current.CancellationToken
            );
    }

    [Fact]
    public async Task End_ProcessesDataModelCorrectly()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var dataModel = new TestDataModel { Value = "Original" };

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);

        _dataClient
            .GetFormData(
                Arg.Any<Guid>(),
                Arg.Any<Type>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<Guid>(),
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Returns(dataModel);

        // Act
        await _sut.End("task1", instance);

        // Assert
        var receivedData =
            _dataClient
                .ReceivedCalls()
                .Single(call => call.GetMethodInfo().Name == nameof(IDataClient.UpdateData))
                .GetArguments()[0]! as TestDataModel;

        receivedData?.Value.ShouldBe("Processed");
    }

    [Fact]
    public async Task End_WorksWithCompleteWorkflow()
    {
        // Arrange
        var instance = AltinnData.CreateTestInstance();
        var application = AltinnData.CreateTestApplication(
            classRef: typeof(TestDataModel).FullName
        );
        var originalData = new TestDataModel { Value = "Original" };

        _applicationClient
            .GetApplication(Arg.Any<string>(), Arg.Any<string>())
            .Returns(application);

        _dataClient
            .GetFormData(
                Arg.Any<Guid>(),
                Arg.Any<Type>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<Guid>(),
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Returns(originalData);

        // Act
        await _sut.End("task1", instance);

        // Assert - verify the complete workflow
        await _applicationClient.Received(1).GetApplication(Arg.Any<string>(), Arg.Any<string>());
        await _dataClient
            .Received(1)
            .GetFormData(
                Arg.Any<Guid>(),
                Arg.Any<Type>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<Guid>(),
                cancellationToken: TestContext.Current.CancellationToken
            );
        await _dataClient
            .Received(1)
            .UpdateData(
                Arg.Any<TestDataModel>(),
                Arg.Any<Guid>(),
                Arg.Any<Type>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<Guid>(),
                cancellationToken: TestContext.Current.CancellationToken
            );
    }

    public class TestDataModel
    {
        public string? Value { get; set; }
    }

    public class TestPreSubmitProcessor : PreSubmitDataModelProcessor<TestDataModel>
    {
        public TestDataModel? LastProcessedDataModel { get; private set; }
        public Instance? LastProcessedInstance { get; private set; }

        public TestPreSubmitProcessor(IDataClient dataClient, IApplicationClient applicationClient)
            : base(dataClient, applicationClient) { }

        protected override Task<TestDataModel> ProcessDataModel(
            TestDataModel currentDataModel,
            Instance instance
        )
        {
            LastProcessedDataModel = currentDataModel;
            LastProcessedInstance = instance;

            return Task.FromResult(new TestDataModel { Value = "Processed" });
        }
    }
}
