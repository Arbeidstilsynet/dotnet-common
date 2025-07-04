using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.Altinn.Abstract.Processing;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class BaseDataProcessorTests
{
    private const string Language = "no";
    private readonly BaseTestDataProcessor _sut;
    private readonly Instance _instance;
    private readonly Guid _dataId;

    public BaseDataProcessorTests()
    {
        _sut = new BaseTestDataProcessor();
        _instance = new Instance();
        _dataId = Guid.NewGuid();
    }

    [Fact]
    public async Task ProcessDataRead_ShouldNotThrow()
    {
        // Arrange
        var data = new BaseTestDataModel { Name = "Test" };

        // Act
        var act = () => _sut.ProcessDataRead(_instance, _dataId, data, Language);

        // Assert
        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ProcessDataWrite_WithCorrectDataType_ShouldCallProcessData()
    {
        // Arrange
        var currentData = new BaseTestDataModel { Name = "Current" };
        var previousData = new BaseTestDataModel { Name = "Previous" };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessDataCalled.ShouldBeTrue();
        _sut.LastCurrentData.ShouldBe(currentData);
        _sut.LastPreviousData.ShouldBe(previousData);
    }

    [Fact]
    public async Task ProcessDataWrite_WithIncorrectDataType_ShouldNotCallProcessData()
    {
        // Arrange
        var wrongData = "This is not a TestDataModel";
        var previousData = new BaseTestDataModel { Name = "Previous" };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, wrongData, previousData, Language);

        // Assert
        _sut.ProcessDataCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ProcessDataWrite_WithNullPreviousData_ShouldCallProcessDataWithNull()
    {
        // Arrange
        var currentData = new BaseTestDataModel { Name = "Current" };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, null, Language);

        // Assert
        _sut.ProcessDataCalled.ShouldBeTrue();
        _sut.LastCurrentData.ShouldBe(currentData);
        _sut.LastPreviousData.ShouldBeNull();
    }

    [Fact]
    public async Task ProcessDataWrite_WithWrongTypeOfPreviousData_ShouldCastToNull()
    {
        // Arrange
        var currentData = new BaseTestDataModel { Name = "Current" };
        var wrongPreviousData = "This is not a TestDataModel";

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, wrongPreviousData, Language);

        // Assert
        _sut.ProcessDataCalled.ShouldBeTrue();
        _sut.LastCurrentData.ShouldBe(currentData);
        _sut.LastPreviousData.ShouldBeNull();
    }
}

// Test implementation of the abstract class
public class BaseTestDataProcessor : BaseDataProcessor<BaseTestDataModel>
{
    public bool ProcessDataCalled { get; private set; }
    public BaseTestDataModel? LastCurrentData { get; private set; }
    public BaseTestDataModel? LastPreviousData { get; private set; }

    protected override Task ProcessData(
        BaseTestDataModel currentDataModel,
        BaseTestDataModel? previousDataModel
    )
    {
        ProcessDataCalled = true;
        LastCurrentData = currentDataModel;
        LastPreviousData = previousDataModel;
        return Task.CompletedTask;
    }
}

// Test data model
public class BaseTestDataModel
{
    public string Name { get; init; } = string.Empty;

    public override bool Equals(object? obj)
    {
        return obj is BaseTestDataModel other && Name == other.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}
