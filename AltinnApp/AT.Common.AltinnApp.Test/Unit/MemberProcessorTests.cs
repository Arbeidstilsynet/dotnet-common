using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Abstract.Processing;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class MemberProcessorTests
{
    private const string Language = "nb";
    private readonly Guid _dataId = Guid.NewGuid();
    private readonly Instance _instance = new();
    private readonly MemberTestProcessor _sut;

    public MemberProcessorTests()
    {
        _sut = new MemberTestProcessor();
    }

    [Fact]
    public async Task ProcessData_WithNullPreviousData_ShouldNotCallProcessMember()
    {
        // Arrange
        var currentData = new MemberTestDataModel { Name = "Current", Value = "CurrentValue" };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, null, Language);

        // Assert
        _sut.ProcessMemberCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ProcessData_WithBothMembersNull_ShouldNotCallProcessMember()
    {
        // Arrange
        var currentData = new MemberTestDataModel { Name = "Current", Value = null };
        var previousData = new MemberTestDataModel { Name = "Previous", Value = null };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessMemberCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ProcessData_WithEqualMembers_ShouldNotCallProcessMember()
    {
        // Arrange
        var currentData = new MemberTestDataModel { Name = "Current", Value = "SameValue" };
        var previousData = new MemberTestDataModel { Name = "Previous", Value = "SameValue" };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessMemberCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ProcessData_WithDifferentMembers_ShouldCallProcessMember()
    {
        // Arrange
        var currentData = new MemberTestDataModel { Name = "Current", Value = "CurrentValue" };
        var previousData = new MemberTestDataModel { Name = "Previous", Value = "PreviousValue" };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessMemberCalled.ShouldBeTrue();
        _sut.LastCurrentMember.ShouldBe("CurrentValue");
        _sut.LastPreviousMember.ShouldBe("PreviousValue");
        _sut.LastCurrentDataModel.ShouldBe(currentData);
        _sut.LastPreviousDataModel.ShouldBe(previousData);
    }

    [Fact]
    public async Task ProcessData_WithCurrentMemberNullAndPreviousNotNull_ShouldCallProcessMember()
    {
        // Arrange
        var currentData = new MemberTestDataModel { Name = "Current", Value = null };
        var previousData = new MemberTestDataModel { Name = "Previous", Value = "PreviousValue" };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessMemberCalled.ShouldBeTrue();
        _sut.LastCurrentMember.ShouldBeNull();
        _sut.LastPreviousMember.ShouldBe("PreviousValue");
    }

    [Fact]
    public async Task ProcessData_WithCurrentMemberNotNullAndPreviousNull_ShouldCallProcessMember()
    {
        // Arrange
        var currentData = new MemberTestDataModel { Name = "Current", Value = "CurrentValue" };
        var previousData = new MemberTestDataModel { Name = "Previous", Value = null };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessMemberCalled.ShouldBeTrue();
        _sut.LastCurrentMember.ShouldBe("CurrentValue");
        _sut.LastPreviousMember.ShouldBeNull();
    }
}

// Test implementation of the abstract class
public class MemberTestProcessor : MemberProcessor<MemberTestDataModel, string>
{
    public bool ProcessMemberCalled { get; private set; }
    public string? LastCurrentMember { get; private set; }
    public string? LastPreviousMember { get; private set; }
    public MemberTestDataModel? LastCurrentDataModel { get; private set; }
    public MemberTestDataModel? LastPreviousDataModel { get; private set; }

    protected override string? AccessMember(MemberTestDataModel dataModel)
    {
        return dataModel.Value;
    }

    protected override Task ProcessMember(
        string? currentMember,
        string? previousMember,
        MemberTestDataModel currentDataModel,
        MemberTestDataModel previousDataModel
    )
    {
        ProcessMemberCalled = true;
        LastCurrentMember = currentMember;
        LastPreviousMember = previousMember;
        LastCurrentDataModel = currentDataModel;
        LastPreviousDataModel = previousDataModel;
        return Task.CompletedTask;
    }
}

// Test data model
public class MemberTestDataModel
{
    public string Name { get; init; } = string.Empty;
    public string? Value { get; init; }

    public override bool Equals(object? obj)
    {
        return obj is MemberTestDataModel other && Name == other.Name && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Value);
    }
}
