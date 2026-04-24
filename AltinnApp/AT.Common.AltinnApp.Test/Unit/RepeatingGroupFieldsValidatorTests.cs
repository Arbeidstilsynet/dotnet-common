using System.Linq.Expressions;
using Altinn.App.Core.Models.Validation;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Abstract;
using Arbeidstilsynet.Common.AltinnApp.Test.Unit.TestFixtures;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class RepeatingGroupFieldsValidatorTests
{
    private readonly Instance _instance = AltinnData.CreateTestInstance();
    private readonly DataElement _dataElement = new() { Id = Guid.NewGuid().ToString() };

    [Fact]
    public async Task ValidateFormData_WithFieldAccessor_PathIsCollectionIndexedWithField()
    {
        // Arrange
        var sut = new ItemNameValidator();
        var model = new OrderModel
        {
            Items = [new OrderItem { Name = "A" }, new OrderItem { Name = "B" }],
        };

        // Act
        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        // Assert
        issues.Count.ShouldBe(2);
        issues[0].Field.ShouldBe("Items[0].Name");
        issues[0].CustomTextKey.ShouldBe("A");
        issues[1].Field.ShouldBe("Items[1].Name");
        issues[1].CustomTextKey.ShouldBe("B");
    }

    [Fact]
    public async Task ValidateFormData_WithoutFieldAccessor_PathIsCollectionIndexed()
    {
        // Arrange
        var sut = new WholeItemValidator();
        var model = new OrderModel
        {
            Items = [new OrderItem { Name = "X" }, new OrderItem { Name = "Y" }],
        };

        // Act
        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        // Assert
        issues.Count.ShouldBe(2);
        issues[0].Field.ShouldBe("Items[0]");
        issues[0].CustomTextKey.ShouldBe("X");
        issues[1].Field.ShouldBe("Items[1]");
        issues[1].CustomTextKey.ShouldBe("Y");
    }

    [Fact]
    public async Task ValidateFormData_NestedCollectionAccessor_PathIncludesFullChain()
    {
        // Arrange
        var sut = new NestedCollectionValidator();
        var model = new NestedModel
        {
            Inner = new InnerModel { Tags = [new Tag { Label = "foo" }] },
        };

        // Act
        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        // Assert
        issues.ShouldHaveSingleItem();
        issues[0].Field.ShouldBe("Inner.Tags[0].Label");
    }

    [Fact]
    public async Task ValidateFormData_EmptyCollection_ReturnsNoIssues()
    {
        var sut = new ItemNameValidator();
        var model = new OrderModel { Items = [] };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateFormData_NullCollection_ReturnsNoIssues()
    {
        var sut = new ItemNameValidator();
        var model = new OrderModel { Items = null! };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateFormData_WrongDataType_ReturnsEmpty()
    {
        var sut = new ItemNameValidator();

        var issues = await sut.ValidateFormData(_instance, _dataElement, "not an OrderModel", null);

        issues.ShouldBeEmpty();
    }

    [Fact]
    public void HasRelevantChanges_WhenItemAdded_ReturnsTrue()
    {
        var sut = new ItemNameValidator();

        var result = sut.HasRelevantChanges(
            new OrderModel { Items = [new OrderItem { Name = "A" }, new OrderItem { Name = "B" }] },
            new OrderModel { Items = [new OrderItem { Name = "A" }] }
        );

        result.ShouldBeTrue();
    }

    [Fact]
    public void HasRelevantChanges_WhenItemFieldChanged_ReturnsTrue()
    {
        var sut = new ItemNameValidator();

        var result = sut.HasRelevantChanges(
            new OrderModel { Items = [new OrderItem { Name = "changed" }] },
            new OrderModel { Items = [new OrderItem { Name = "original" }] }
        );

        result.ShouldBeTrue();
    }

    [Fact]
    public void HasRelevantChanges_WhenUnchanged_ReturnsFalse()
    {
        var sut = new ItemNameValidator();

        var result = sut.HasRelevantChanges(
            new OrderModel { Items = [new OrderItem { Name = "same" }] },
            new OrderModel { Items = [new OrderItem { Name = "same" }] }
        );

        result.ShouldBeFalse();
    }

    [Fact]
    public void HasRelevantChanges_WrongType_ReturnsFalse()
    {
        var sut = new ItemNameValidator();

        sut.HasRelevantChanges("not a model", "also not").ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateFormData_WithoutFieldAccessor_NestedCollection_PathIsCorrect()
    {
        var sut = new NestedWholeTagValidator();
        var model = new NestedModel
        {
            Inner = new InnerModel { Tags = [new Tag { Label = "t1" }, new Tag { Label = "t2" }] },
        };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.Count.ShouldBe(2);
        issues[0].Field.ShouldBe("Inner.Tags[0]");
        issues[1].Field.ShouldBe("Inner.Tags[1]");
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

    /// <summary>
    /// Collection: m.Items, Field: item.Name
    /// </summary>
    private class ItemNameValidator : RepeatingGroupFieldsValidator<OrderModel, OrderItem, string?>
    {
        protected override Expression<
            Func<OrderModel, IEnumerable<OrderItem>>
        > GetCollectionAccessor() => m => m.Items;

        protected override Expression<Func<OrderItem, string?>>? GetFieldAccessor() =>
            item => item.Name;

        protected override Task<List<ValidationIssue>> ValidateField(
            string? value,
            string fieldPath
        ) =>
            Task.FromResult(
                new List<ValidationIssue>
                {
                    new()
                    {
                        Field = fieldPath,
                        Severity = ValidationIssueSeverity.Error,
                        CustomTextKey = value ?? "null",
                    },
                }
            );
    }

    /// <summary>
    /// Collection: m.Items, no field accessor — whole item passed
    /// </summary>
    private class WholeItemValidator
        : RepeatingGroupFieldsValidator<OrderModel, OrderItem, OrderItem>
    {
        protected override Expression<
            Func<OrderModel, IEnumerable<OrderItem>>
        > GetCollectionAccessor() => m => m.Items;

        protected override Task<List<ValidationIssue>> ValidateField(
            OrderItem? value,
            string fieldPath
        ) =>
            Task.FromResult(
                new List<ValidationIssue>
                {
                    new()
                    {
                        Field = fieldPath,
                        Severity = ValidationIssueSeverity.Error,
                        CustomTextKey = value?.Name ?? "null",
                    },
                }
            );
    }

    /// <summary>
    /// Nested collection: m.Inner.Tags, Field: tag.Label
    /// </summary>
    private class NestedCollectionValidator
        : RepeatingGroupFieldsValidator<NestedModel, Tag, string?>
    {
        protected override Expression<
            Func<NestedModel, IEnumerable<Tag>>
        > GetCollectionAccessor() => m => m.Inner.Tags;

        protected override Expression<Func<Tag, string?>>? GetFieldAccessor() => tag => tag.Label;

        protected override Task<List<ValidationIssue>> ValidateField(
            string? value,
            string fieldPath
        ) =>
            Task.FromResult(
                new List<ValidationIssue>
                {
                    new()
                    {
                        Field = fieldPath,
                        Severity = ValidationIssueSeverity.Error,
                        CustomTextKey = value ?? "null",
                    },
                }
            );
    }

    /// <summary>
    /// Nested collection without field accessor: m.Inner.Tags, whole Tag passed
    /// </summary>
    private class NestedWholeTagValidator : RepeatingGroupFieldsValidator<NestedModel, Tag, Tag>
    {
        protected override Expression<
            Func<NestedModel, IEnumerable<Tag>>
        > GetCollectionAccessor() => m => m.Inner.Tags;

        protected override Task<List<ValidationIssue>> ValidateField(
            Tag? value,
            string fieldPath
        ) =>
            Task.FromResult(
                new List<ValidationIssue>
                {
                    new()
                    {
                        Field = fieldPath,
                        Severity = ValidationIssueSeverity.Error,
                        CustomTextKey = value?.Label ?? "null",
                    },
                }
            );
    }

    #endregion
}
