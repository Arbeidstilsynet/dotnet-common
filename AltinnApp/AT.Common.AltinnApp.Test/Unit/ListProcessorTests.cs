using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Abstract.Processing;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class ListProcessorTests
{
    private const string Language = "nb";
    private readonly Guid _dataId = Guid.NewGuid();
    private readonly Instance _instance = new();
    private readonly ListTestProcessor _sut;

    public ListProcessorTests()
    {
        _sut = new ListTestProcessor();
    }

    [Fact]
    public async Task ProcessMember_WithAddedItem_ShouldCallProcessItem()
    {
        // Arrange
        var item1Id = Guid.NewGuid();
        var item2Id = Guid.NewGuid();
        var item3Id = Guid.NewGuid();

        var currentData = new ListTestDataModel
        {
            Name = "Current",
            Items =
            [
                new TestListItem { AltinnRowId = item1Id, Value = "Item1" },
                new TestListItem { AltinnRowId = item2Id, Value = "Item2" },
                new TestListItem { AltinnRowId = item3Id, Value = "Item3" },
            ],
        };
        var previousData = new ListTestDataModel
        {
            Name = "Previous",
            Items =
            [
                new TestListItem { AltinnRowId = item1Id, Value = "Item1" },
                new TestListItem { AltinnRowId = item2Id, Value = "Item2" },
            ],
        };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessItemCalled.ShouldBeTrue();
        _sut.ProcessItemCallCount.ShouldBe(1);
        _sut.LastCurrentItem.ShouldNotBeNull();
        _sut.LastCurrentItem!.AltinnRowId.ShouldBe(item3Id);
        _sut.LastPreviousItem.ShouldBeNull();
    }

    [Fact]
    public async Task ProcessMember_WithModifiedItem_ShouldCallProcessItem()
    {
        // Arrange
        var item1Id = Guid.NewGuid();
        var item2Id = Guid.NewGuid();

        var currentData = new ListTestDataModel
        {
            Name = "Current",
            Items =
            [
                new TestListItem { AltinnRowId = item1Id, Value = "Item1" },
                new TestListItem { AltinnRowId = item2Id, Value = "ChangedItem2" },
            ],
        };
        var previousData = new ListTestDataModel
        {
            Name = "Previous",
            Items =
            [
                new TestListItem { AltinnRowId = item1Id, Value = "Item1" },
                new TestListItem { AltinnRowId = item2Id, Value = "OriginalItem2" },
            ],
        };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessItemCalled.ShouldBeTrue();
        _sut.ProcessItemCallCount.ShouldBe(1);
        _sut.LastCurrentItem!.Value.ShouldBe("ChangedItem2");
        _sut.LastPreviousItem!.Value.ShouldBe("OriginalItem2");
    }

    [Fact]
    public async Task ProcessMember_WithIdenticalLists_ShouldNotCallProcessItem()
    {
        // Arrange
        var item1Id = Guid.NewGuid();
        var item2Id = Guid.NewGuid();

        var currentData = new ListTestDataModel
        {
            Name = "Current",
            Items =
            [
                new TestListItem { AltinnRowId = item1Id, Value = "Item1" },
                new TestListItem { AltinnRowId = item2Id, Value = "Item2" },
            ],
        };
        var previousData = new ListTestDataModel
        {
            Name = "Previous",
            Items =
            [
                new TestListItem { AltinnRowId = item1Id, Value = "Item1" },
                new TestListItem { AltinnRowId = item2Id, Value = "Item2" },
            ],
        };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessItemCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ProcessMember_WithRemovedItem_ShouldCallProcessItem()
    {
        // Arrange
        var item1Id = Guid.NewGuid();
        var item2Id = Guid.NewGuid();

        var currentData = new ListTestDataModel
        {
            Name = "Current",
            Items = [new TestListItem { AltinnRowId = item1Id, Value = "Item1" }],
        };
        var previousData = new ListTestDataModel
        {
            Name = "Previous",
            Items =
            [
                new TestListItem { AltinnRowId = item1Id, Value = "Item1" },
                new TestListItem { AltinnRowId = item2Id, Value = "Item2" },
            ],
        };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessItemCalled.ShouldBeTrue();
        _sut.ProcessItemCallCount.ShouldBe(1);
        _sut.LastCurrentItem.ShouldBeNull();
        _sut.LastPreviousItem.ShouldNotBeNull();
        _sut.LastPreviousItem!.AltinnRowId.ShouldBe(item2Id);
    }

    [Fact]
    public async Task ProcessMember_WithNullCurrentList_ShouldCallProcessItemForEachRemovedItem()
    {
        // Arrange
        var item1Id = Guid.NewGuid();

        var currentData = new ListTestDataModel { Name = "Current", Items = null };
        var previousData = new ListTestDataModel
        {
            Name = "Previous",
            Items = [new TestListItem { AltinnRowId = item1Id, Value = "Item1" }],
        };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessItemCalled.ShouldBeTrue();
        _sut.ProcessItemCallCount.ShouldBe(1);
        _sut.LastCurrentItem.ShouldBeNull();
        _sut.LastPreviousItem.ShouldNotBeNull();
    }

    [Fact]
    public async Task ProcessMember_WithNullPreviousList_ShouldCallProcessItemForEachAddedItem()
    {
        // Arrange
        var item1Id = Guid.NewGuid();

        var currentData = new ListTestDataModel
        {
            Name = "Current",
            Items = [new TestListItem { AltinnRowId = item1Id, Value = "Item1" }],
        };
        var previousData = new ListTestDataModel { Name = "Previous", Items = null };

        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessItemCalled.ShouldBeTrue();
        _sut.ProcessItemCallCount.ShouldBe(1);
        _sut.LastCurrentItem.ShouldNotBeNull();
        _sut.LastPreviousItem.ShouldBeNull();
    }
}

// Test implementation of the abstract class
public class ListTestProcessor : ListProcessor<ListTestDataModel, TestListItem>
{
    public bool ProcessItemCalled { get; private set; }
    public int ProcessItemCallCount { get; private set; }

    public TestListItem? LastCurrentItem { get; private set; }
    public TestListItem? LastPreviousItem { get; private set; }
    public ListTestDataModel? LastCurrentDataModel { get; private set; }
    public ListTestDataModel? LastPreviousDataModel { get; private set; }

    protected override List<TestListItem>? AccessMember(ListTestDataModel dataModel)
    {
        return dataModel.Items;
    }

    protected override Task ProcessItem(
        TestListItem? currentItem,
        TestListItem? previousItem,
        ListTestDataModel currentDataModel,
        ListTestDataModel previousDataModel
    )
    {
        ProcessItemCalled = true;
        ProcessItemCallCount++;
        LastCurrentItem = currentItem;
        LastPreviousItem = previousItem;
        LastCurrentDataModel = currentDataModel;
        LastPreviousDataModel = previousDataModel;
        return Task.CompletedTask;
    }
}

// Test list item with AltinnRowId
public class TestListItem
{
    public Guid AltinnRowId { get; set; }
    public string Value { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is not TestListItem other)
            return false;
        return AltinnRowId == other.AltinnRowId && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AltinnRowId, Value);
    }
}

// Test data model
public class ListTestDataModel
{
    public string Name { get; set; } = string.Empty;
    public List<TestListItem>? Items { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not ListTestDataModel other)
            return false;

        if (Items == null && other.Items == null)
            return Name == other.Name;
        if (Items == null || other.Items == null)
            return false;

        return Name == other.Name && Items.SequenceEqual(other.Items);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Items);
    }
}
