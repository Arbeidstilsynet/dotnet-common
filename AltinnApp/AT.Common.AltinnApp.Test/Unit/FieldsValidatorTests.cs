using System.Linq.Expressions;
using Altinn.App.Core.Models.Validation;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Abstract;
using Arbeidstilsynet.Common.AltinnApp.Test.Unit.TestFixtures;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class FieldsValidatorTests
{
    private readonly Instance _instance = AltinnData.CreateTestInstance();
    private readonly DataElement _dataElement = new() { Id = Guid.NewGuid().ToString() };

    [Fact]
    public async Task ValidateFormData_PassesFieldPathFromMemberExpression()
    {
        // Arrange
        var sut = new SimpleFieldsValidator(
            [m => m.Name],
            (_, path) =>
                Task.FromResult(
                    new List<ValidationIssue>
                    {
                        new()
                        {
                            Field = path,
                            Severity = ValidationIssueSeverity.Error,
                            CustomTextKey = "test",
                        },
                    }
                )
        );

        // Act
        var issues = await sut.ValidateFormData(
            _instance,
            _dataElement,
            new TestDataModel { Name = "value" },
            null
        );

        // Assert — the path is the member chain without the parameter name
        issues.ShouldHaveSingleItem();
        issues[0].Field.ShouldBe("Name");
    }

    [Fact]
    public async Task ValidateFormData_NestedProperty_PathIncludesFullChain()
    {
        // Arrange
        var sut = new NestedFieldsValidator(
            [x => x.Inner.Value],
            (_, path) =>
                Task.FromResult(
                    new List<ValidationIssue>
                    {
                        new()
                        {
                            Field = path,
                            Severity = ValidationIssueSeverity.Error,
                            CustomTextKey = "test",
                        },
                    }
                )
        );

        // Act
        var issues = await sut.ValidateFormData(
            _instance,
            _dataElement,
            new NestedModel { Inner = new InnerModel { Value = "v" } },
            null
        );

        // Assert
        issues.ShouldHaveSingleItem();
        issues[0].Field.ShouldBe("Inner.Value");
    }

    [Fact]
    public async Task ValidateFormData_ParameterNameIsStrippedFromPath()
    {
        // Demonstrates that different parameter names produce the same path
        var sut = new SimpleFieldsValidator(
            [dataModel => dataModel.Name],
            (_, path) =>
                Task.FromResult(
                    new List<ValidationIssue>
                    {
                        new()
                        {
                            Field = path,
                            Severity = ValidationIssueSeverity.Error,
                            CustomTextKey = "test",
                        },
                    }
                )
        );

        var issues = await sut.ValidateFormData(
            _instance,
            _dataElement,
            new TestDataModel { Name = "value" },
            null
        );

        issues.ShouldHaveSingleItem();
        issues[0].Field.ShouldBe("Name");
    }

    [Fact]
    public async Task ValidateFormData_MultipleFields_ValidatesEach()
    {
        // Arrange
        var sut = new MultiFieldsValidator(
            [m => m.Name, m => m.Other],
            (_, path) =>
                Task.FromResult(
                    new List<ValidationIssue>
                    {
                        new()
                        {
                            Field = path,
                            Severity = ValidationIssueSeverity.Error,
                            CustomTextKey = "err",
                        },
                    }
                )
        );

        // Act
        var issues = await sut.ValidateFormData(
            _instance,
            _dataElement,
            new MultiFieldModel { Name = "a", Other = "b" },
            null
        );

        // Assert
        issues.Count.ShouldBe(2);
        issues.ShouldContain(i => i.Field == "Name");
        issues.ShouldContain(i => i.Field == "Other");
    }

    [Fact]
    public async Task ValidateFormData_WhenNoIssues_ReturnsEmptyList()
    {
        var sut = new SimpleFieldsValidator(
            [m => m.Name],
            (_, _) => Task.FromResult(new List<ValidationIssue>())
        );

        var issues = await sut.ValidateFormData(
            _instance,
            _dataElement,
            new TestDataModel { Name = "ok" },
            null
        );

        issues.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateFormData_WhenDataIsWrongType_ReturnsEmptyList()
    {
        var sut = new SimpleFieldsValidator(
            [m => m.Name],
            (_, _) =>
                Task.FromResult(
                    new List<ValidationIssue>
                    {
                        new()
                        {
                            Field = "should-not-appear",
                            Severity = ValidationIssueSeverity.Error,
                            CustomTextKey = "err",
                        },
                    }
                )
        );

        var issues = await sut.ValidateFormData(
            _instance,
            _dataElement,
            "not a TestDataModel",
            null
        );

        issues.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateFormData_NullNestedProperty_ReturnsDefaultValue()
    {
        var sut = new NestedFieldsValidator(
            [x => x.Inner.Value],
            (value, path) =>
                Task.FromResult(
                    new List<ValidationIssue>
                    {
                        new()
                        {
                            Field = path,
                            Severity = ValidationIssueSeverity.Error,
                            CustomTextKey = value ?? "was-null",
                        },
                    }
                )
        );

        var issues = await sut.ValidateFormData(
            _instance,
            _dataElement,
            new NestedModel { Inner = null! },
            null
        );

        issues.ShouldHaveSingleItem();
        issues[0].CustomTextKey.ShouldBe("was-null");
    }

    [Fact]
    public void HasRelevantChanges_WhenFieldChanged_ReturnsTrue()
    {
        var sut = new SimpleFieldsValidator(
            [m => m.Name],
            (_, _) => Task.FromResult(new List<ValidationIssue>())
        );

        var result = sut.HasRelevantChanges(
            new TestDataModel { Name = "new" },
            new TestDataModel { Name = "old" }
        );

        result.ShouldBeTrue();
    }

    [Fact]
    public void HasRelevantChanges_WhenFieldUnchanged_ReturnsFalse()
    {
        var sut = new SimpleFieldsValidator(
            [m => m.Name],
            (_, _) => Task.FromResult(new List<ValidationIssue>())
        );

        var result = sut.HasRelevantChanges(
            new TestDataModel { Name = "same" },
            new TestDataModel { Name = "same" }
        );

        result.ShouldBeFalse();
    }

    [Fact]
    public void HasRelevantChanges_WhenWrongType_ReturnsFalse()
    {
        var sut = new SimpleFieldsValidator(
            [m => m.Name],
            (_, _) => Task.FromResult(new List<ValidationIssue>())
        );

        var result = sut.HasRelevantChanges("not a model", "also not a model");

        result.ShouldBeFalse();
    }

    [Fact]
    public void CompileAccessors_NonMemberExpression_Throws()
    {
        // m.Name.Length is a MemberExpression on int, but we need string? — use a method call instead
        var sut = new MethodCallFieldsValidator(
            [m => m.Name!.ToUpper()],
            (_, _) => Task.FromResult(new List<ValidationIssue>())
        );

        Should.Throw<InvalidOperationException>(() =>
            sut.ValidateFormData(_instance, _dataElement, new TestDataModel(), null)
        );
    }

    [Fact]
    public void CompileAccessors_DuplicateField_Throws()
    {
        var sut = new SimpleFieldsValidator(
            [m => m.Name, m => m.Name],
            (_, _) => Task.FromResult(new List<ValidationIssue>())
        );

        Should.Throw<InvalidOperationException>(() =>
            sut.ValidateFormData(_instance, _dataElement, new TestDataModel(), null)
        );
    }

    #region Test helpers

    public class TestDataModel
    {
        public string? Name { get; set; }
    }

    public class MultiFieldModel
    {
        public string? Name { get; set; }
        public string? Other { get; set; }
    }

    public class NestedModel
    {
        public InnerModel Inner { get; set; } = new();
    }

    public class InnerModel
    {
        public string? Value { get; set; }
    }

    private class SimpleFieldsValidator : FieldsValidator<TestDataModel, string?>
    {
        private readonly IEnumerable<Expression<Func<TestDataModel, string?>>> _fields;
        private readonly Func<string?, string, Task<List<ValidationIssue>>> _validateFunc;

        public SimpleFieldsValidator(
            IEnumerable<Expression<Func<TestDataModel, string?>>> fields,
            Func<string?, string, Task<List<ValidationIssue>>> validateFunc
        )
        {
            _fields = fields;
            _validateFunc = validateFunc;
        }

        protected override IEnumerable<
            Expression<Func<TestDataModel, string?>>
        > GetRelevantFields() => _fields;

        protected override Task<List<ValidationIssue>> ValidateField(
            string? value,
            string fieldPath
        ) => _validateFunc(value, fieldPath);
    }

    private class MultiFieldsValidator : FieldsValidator<MultiFieldModel, string?>
    {
        private readonly IEnumerable<Expression<Func<MultiFieldModel, string?>>> _fields;
        private readonly Func<string?, string, Task<List<ValidationIssue>>> _validateFunc;

        public MultiFieldsValidator(
            IEnumerable<Expression<Func<MultiFieldModel, string?>>> fields,
            Func<string?, string, Task<List<ValidationIssue>>> validateFunc
        )
        {
            _fields = fields;
            _validateFunc = validateFunc;
        }

        protected override IEnumerable<
            Expression<Func<MultiFieldModel, string?>>
        > GetRelevantFields() => _fields;

        protected override Task<List<ValidationIssue>> ValidateField(
            string? value,
            string fieldPath
        ) => _validateFunc(value, fieldPath);
    }

    private class MethodCallFieldsValidator : FieldsValidator<TestDataModel, string?>
    {
        private readonly IEnumerable<Expression<Func<TestDataModel, string?>>> _fields;
        private readonly Func<string?, string, Task<List<ValidationIssue>>> _validateFunc;

        public MethodCallFieldsValidator(
            IEnumerable<Expression<Func<TestDataModel, string?>>> fields,
            Func<string?, string, Task<List<ValidationIssue>>> validateFunc
        )
        {
            _fields = fields;
            _validateFunc = validateFunc;
        }

        protected override IEnumerable<
            Expression<Func<TestDataModel, string?>>
        > GetRelevantFields() => _fields;

        protected override Task<List<ValidationIssue>> ValidateField(
            string? value,
            string fieldPath
        ) => _validateFunc(value, fieldPath);
    }

    private class NestedFieldsValidator : FieldsValidator<NestedModel, string?>
    {
        private readonly IEnumerable<Expression<Func<NestedModel, string?>>> _fields;
        private readonly Func<string?, string, Task<List<ValidationIssue>>> _validateFunc;

        public NestedFieldsValidator(
            IEnumerable<Expression<Func<NestedModel, string?>>> fields,
            Func<string?, string, Task<List<ValidationIssue>>> validateFunc
        )
        {
            _fields = fields;
            _validateFunc = validateFunc;
        }

        protected override IEnumerable<
            Expression<Func<NestedModel, string?>>
        > GetRelevantFields() => _fields;

        protected override Task<List<ValidationIssue>> ValidateField(
            string? value,
            string fieldPath
        ) => _validateFunc(value, fieldPath);
    }

    #endregion
}
