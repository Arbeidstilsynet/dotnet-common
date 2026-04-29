using System.Linq.Expressions;
using Altinn.App.Core.Models.Validation;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Abstract;
using Arbeidstilsynet.Common.AltinnApp.Test.Unit.TestFixtures;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class RepeatingGroupValidatorTests
{
    private readonly Instance _instance = AltinnData.CreateTestInstance();
    private readonly DataElement _dataElement = new() { Id = Guid.NewGuid().ToString() };

    [Fact]
    public async Task ValidateFormData_ValidatesEachItemWithIndexedPath()
    {
        // Arrange
        var sut = new OrderItemValidator();
        var model = new OrderModel
        {
            Items = [new OrderItem { Name = "A" }, new OrderItem { Name = "B" }],
        };

        // Act
        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        // Assert
        issues.Count.ShouldBe(2);
        issues[0].Field.ShouldBe("Items[0]");
        issues[0].CustomTextKey.ShouldBe("A");
        issues[1].Field.ShouldBe("Items[1]");
        issues[1].CustomTextKey.ShouldBe("B");
    }

    [Fact]
    public async Task ValidateFormData_NestedCollection_PathIncludesFullChain()
    {
        // Arrange
        var sut = new NestedTagValidator();
        var model = new NestedModel
        {
            Inner = new InnerModel { Tags = [new Tag { Label = "foo" }] },
        };

        // Act
        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        // Assert
        issues.ShouldHaveSingleItem();
        issues[0].Field.ShouldBe("Inner.Tags[0]");
        issues[0].CustomTextKey.ShouldBe("foo");
    }

    [Fact]
    public async Task ValidateFormData_EmptyCollection_ReturnsNoIssues()
    {
        var sut = new OrderItemValidator();
        var model = new OrderModel { Items = [] };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateFormData_NullCollection_ReturnsNoIssues()
    {
        var sut = new OrderItemValidator();
        var model = new OrderModel { Items = null! };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateFormData_WrongDataType_ReturnsEmpty()
    {
        var sut = new OrderItemValidator();

        var issues = await sut.ValidateFormData(_instance, _dataElement, "not an OrderModel", null);

        issues.ShouldBeEmpty();
    }

    [Fact]
    public void HasRelevantChanges_WhenItemAdded_ReturnsTrue()
    {
        var sut = new OrderItemValidator();

        var result = sut.HasRelevantChanges(
            new OrderModel { Items = [new OrderItem { Name = "A" }, new OrderItem { Name = "B" }] },
            new OrderModel { Items = [new OrderItem { Name = "A" }] }
        );

        result.ShouldBeTrue();
    }

    [Fact]
    public void HasRelevantChanges_WhenItemChanged_ReturnsTrue()
    {
        var sut = new OrderItemValidator();

        var result = sut.HasRelevantChanges(
            new OrderModel { Items = [new OrderItem { Name = "changed" }] },
            new OrderModel { Items = [new OrderItem { Name = "original" }] }
        );

        result.ShouldBeTrue();
    }

    [Fact]
    public void HasRelevantChanges_WhenUnchanged_ReturnsFalse()
    {
        var sut = new OrderItemValidator();
        var sharedItems = new List<OrderItem> { new() { Name = "same" } };

        var result = sut.HasRelevantChanges(
            new OrderModel { Items = sharedItems },
            new OrderModel { Items = sharedItems }
        );

        result.ShouldBeFalse();
    }

    [Fact]
    public void HasRelevantChanges_WrongType_ReturnsFalse()
    {
        var sut = new OrderItemValidator();

        sut.HasRelevantChanges("not a model", "also not").ShouldBeFalse();
    }

    #region Test models

    public class OrderModel
    {
        public List<OrderItem> Items { get; set; } = [];
    }

    public class OrderItem
    {
        public string? Name { get; set; }
    }

    public class NestedModel
    {
        public InnerModel Inner { get; set; } = new();
    }

    public class InnerModel
    {
        public List<Tag> Tags { get; set; } = [];
    }

    public class Tag
    {
        public string? Label { get; set; }
    }

    #endregion

    #region Test validators

    private class OrderItemValidator : RepeatingGroupValidator<OrderModel, OrderItem>
    {
        protected override Expression<
            Func<OrderModel, IEnumerable<OrderItem>>
        > GetCollectionAccessor() => m => m.Items;

        protected override Task<List<ValidationIssue>> ValidateItem(
            OrderItem? item,
            string itemPath
        ) =>
            Task.FromResult(
                new List<ValidationIssue>
                {
                    new()
                    {
                        Field = itemPath,
                        Severity = ValidationIssueSeverity.Error,
                        CustomTextKey = item?.Name ?? "null",
                    },
                }
            );
    }

    private class NestedTagValidator : RepeatingGroupValidator<NestedModel, Tag>
    {
        protected override Expression<
            Func<NestedModel, IEnumerable<Tag>>
        > GetCollectionAccessor() => m => m.Inner.Tags;

        protected override Task<List<ValidationIssue>> ValidateItem(Tag? item, string itemPath) =>
            Task.FromResult(
                new List<ValidationIssue>
                {
                    new()
                    {
                        Field = itemPath,
                        Severity = ValidationIssueSeverity.Error,
                        CustomTextKey = item?.Label ?? "null",
                    },
                }
            );
    }

    #endregion
}
