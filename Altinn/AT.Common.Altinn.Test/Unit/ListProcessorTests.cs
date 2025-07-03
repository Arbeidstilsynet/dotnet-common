using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.Altinn.Abstract.Processing;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class ListProcessorTests
{
    private const string Language = "nb";
    private readonly ListTestProcessor _sut;
    private readonly Instance _instance = new Instance();
    private readonly Guid _dataId = Guid.NewGuid();

    public ListProcessorTests()
    {
        _sut = new ListTestProcessor();
    }

    [Fact]
    public async Task ProcessMember_WithDifferentListCounts_ShouldCallProcessListChange()
    {
        // Arrange
        var currentData = new ListTestDataModel 
        { 
            Name = "Current", 
            Items = ["Item1", "Item2", "Item3"] 
        };
        var previousData = new ListTestDataModel 
        { 
            Name = "Previous", 
            Items = ["Item1", "Item2"] 
        };
        
        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessListChangeCalled.ShouldBeTrue();
        _sut.ProcessItemCalled.ShouldBeFalse();
        _sut.LastCurrentList.ShouldBe(currentData.Items);
        _sut.LastPreviousList.ShouldBe(previousData.Items);
        _sut.LastCurrentDataModel.ShouldBe(currentData);
        _sut.LastPreviousDataModel.ShouldBe(previousData);
    }

    [Fact]
    public async Task ProcessMember_WithSameCountButDifferentItems_ShouldCallProcessItem()
    {
        // Arrange
        var currentData = new ListTestDataModel 
        { 
            Name = "Current", 
            Items = ["Item1", "ChangedItem2"]
        };
        var previousData = new ListTestDataModel 
        { 
            Name = "Previous", 
            Items = ["Item1", "OriginalItem2"]
        };
        
        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);

        // Assert
        _sut.ProcessListChangeCalled.ShouldBeFalse();
        _sut.ProcessItemCalled.ShouldBeTrue();
        _sut.ProcessItemCallCount.ShouldBe(1); // Only one item changed
        _sut.LastCurrentItem.ShouldBe("ChangedItem2");
        _sut.LastPreviousItem.ShouldBe("OriginalItem2");
    }

    [Fact]
    public async Task ProcessMember_WithIdenticalLists_ShouldNotCallAnyProcessMethods()
    {
        // Arrange
        var currentData = new ListTestDataModel 
        { 
            Name = "Current", 
            Items = ["Item1", "Item2"]
        };
        var previousData = new ListTestDataModel 
        { 
            Name = "Previous", 
            Items = ["Item1", "Item2"]
        };
        
        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);
        
        // Assert
        _sut.ProcessListChangeCalled.ShouldBeFalse();
        _sut.ProcessItemCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ProcessMember_WithNullCurrentAndNullPreviousItems_ShouldNotCallProcessItem()
    {
        // Arrange
        var currentData = new ListTestDataModel 
        { 
            Name = "Current", 
            Items = [null!, "Item2"]
        };
        var previousData = new ListTestDataModel 
        { 
            Name = "Previous", 
            Items = [null!, "Item2"]
        };
        
        var sut = new ListTestProcessorNullable();
        
        // Act
        await sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);
        
        // Assert
        sut.ProcessItemCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ProcessMember_WithOneNullItem_ShouldCallProcessItem()
    {
        // Arrange
        var currentData = new ListTestDataModel 
        { 
            Name = "Current", 
            Items = [null!, "Item2"] 
        };
        var previousData = new ListTestDataModel 
        { 
            Name = "Previous", 
            Items = ["Item1", "Item2"] 
        };
        
        var sut = new ListTestProcessorNullable();
        
        // Act
        await sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);
        
        // Assert
        sut.ProcessItemCalled.ShouldBeTrue();
        sut.LastCurrentItem.ShouldBeNull();
        sut.LastPreviousItem.ShouldBe("Item1");
    }

    [Fact]
    public async Task ProcessMember_WithNullLists_ShouldCallProcessListChange()
    {
        // Arrange
        var currentData = new ListTestDataModel { Name = "Current", Items = null };
        var previousData = new ListTestDataModel { Name = "Previous", Items = new List<string> { "Item1" } };
        
        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, currentData, previousData, Language);
        
        // Assert
        _sut.ProcessListChangeCalled.ShouldBeTrue();
        _sut.LastCurrentList.ShouldBeNull();
        _sut.LastPreviousList.ShouldBe(previousData.Items);
    }
}

// Test implementation of the abstract class
public class ListTestProcessor : ListProcessor<ListTestDataModel, string>
{
    public bool ProcessListChangeCalled { get; private set; }
    public bool ProcessItemCalled { get; private set; }
    public int ProcessItemCallCount { get; private set; }
    
    public List<string>? LastCurrentList { get; private set; }
    public List<string>? LastPreviousList { get; private set; }
    public ListTestDataModel? LastCurrentDataModel { get; private set; }
    public ListTestDataModel? LastPreviousDataModel { get; private set; }
    
    public string? LastCurrentItem { get; private set; }
    public string? LastPreviousItem { get; private set; }

    protected override List<string>? AccessMember(ListTestDataModel dataModel)
    {
        return dataModel.Items;
    }

    protected override Task ProcessListChange(List<string>? currentList, List<string>? previousList, ListTestDataModel currentDataModel, ListTestDataModel previousDataModel)
    {
        ProcessListChangeCalled = true;
        LastCurrentList = currentList;
        LastPreviousList = previousList;
        LastCurrentDataModel = currentDataModel;
        LastPreviousDataModel = previousDataModel;
        return Task.CompletedTask;
    }

    protected override Task ProcessItem(string currentItem, string previousItem, ListTestDataModel currentDataModel, ListTestDataModel previousDataModel)
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

// Test implementation for nullable items
public class ListTestProcessorNullable : ListProcessor<ListTestDataModel, string?>
{
    public bool ProcessItemCalled { get; private set; }
    public string? LastCurrentItem { get; private set; }
    public string? LastPreviousItem { get; private set; }

    protected override List<string?>? AccessMember(ListTestDataModel dataModel)
    {
        return dataModel.Items?.Cast<string?>().ToList();
    }

    protected override Task ProcessItem(string? currentItem, string? previousItem, ListTestDataModel currentDataModel, ListTestDataModel previousDataModel)
    {
        ProcessItemCalled = true;
        LastCurrentItem = currentItem;
        LastPreviousItem = previousItem;
        return Task.CompletedTask;
    }

    // Expose the protected method for testing
    public Task TestProcessMember(List<string?>? currentList, List<string?>? previousList, ListTestDataModel currentDataModel, ListTestDataModel previousDataModel)
    {
        return ProcessMember(currentList, previousList, currentDataModel, previousDataModel);
    }
}

// Test data model
public class ListTestDataModel
{
    public string Name { get; set; } = string.Empty;
    public List<string>? Items { get; set; }
    
    public override bool Equals(object? obj)
    {
        if (obj is not ListTestDataModel other) return false;
        
        if (Items == null && other.Items == null) return Name == other.Name;
        if (Items == null || other.Items == null) return false;
        
        return Name == other.Name && Items.SequenceEqual(other.Items);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Items);
    }
}
