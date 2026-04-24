using System.ComponentModel.DataAnnotations;
using Arbeidstilsynet.Common.AltinnApp.Implementation;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shouldly;
using Xunit;
using ValidationException = FluentValidation.ValidationException;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class StructuredDataValidatorTests
{
    [Fact]
    public async Task ValidateAndThrow_WithFluentValidator_WhenValid_ShouldNotThrow()
    {
        // Arrange
        var validator = Substitute.For<IValidator<TestModel>>();
        validator
            .ValidateAsync(Arg.Any<TestModel>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var sut = new StructuredDataValidator<TestModel>([validator]);

        // Act & Assert
        await Should.NotThrowAsync(() => sut.ValidateAndThrow(new TestModel { Name = "Valid" }));
    }

    [Fact]
    public async Task ValidateAndThrow_WithFluentValidator_WhenInvalid_ShouldThrowValidationException()
    {
        // Arrange
        var validator = Substitute.For<IValidator<TestModel>>();
        validator
            .ValidateAsync(Arg.Any<TestModel>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "Name is required")]));

        var sut = new StructuredDataValidator<TestModel>([validator]);

        // Act & Assert
        var ex = await Should.ThrowAsync<ValidationException>(
            () => sut.ValidateAndThrow(new TestModel())
        );
        ex.Errors.ShouldHaveSingleItem();
        ex.Errors.First().PropertyName.ShouldBe("Name");
    }

    [Fact]
    public async Task ValidateAndThrow_WithMultipleFluentValidators_AggregatesFailures()
    {
        // Arrange
        var validator1 = Substitute.For<IValidator<TestModel>>();
        validator1
            .ValidateAsync(Arg.Any<TestModel>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "Name is required")]));

        var validator2 = Substitute.For<IValidator<TestModel>>();
        validator2
            .ValidateAsync(Arg.Any<TestModel>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Age", "Age must be positive")]));

        var sut = new StructuredDataValidator<TestModel>([validator1, validator2]);

        // Act & Assert
        var ex = await Should.ThrowAsync<ValidationException>(
            () => sut.ValidateAndThrow(new TestModel())
        );
        ex.Errors.Count().ShouldBe(2);
    }

    [Fact]
    public async Task ValidateAndThrow_WithNoValidators_FallsBackToDataAnnotations_WhenValid()
    {
        // Arrange
        var sut = new StructuredDataValidator<AnnotatedModel>([]);

        // Act & Assert
        await Should.NotThrowAsync(
            () => sut.ValidateAndThrow(new AnnotatedModel { RequiredField = "present" })
        );
    }

    [Fact]
    public async Task ValidateAndThrow_WithNoValidators_FallsBackToDataAnnotations_WhenInvalid()
    {
        // Arrange
        var sut = new StructuredDataValidator<AnnotatedModel>([]);

        // Act & Assert
        var ex = await Should.ThrowAsync<ValidationException>(
            () => sut.ValidateAndThrow(new AnnotatedModel())
        );
        ex.Errors.ShouldContain(e => e.PropertyName.Contains("RequiredField"));
    }

    [Fact]
    public async Task ValidateAndThrow_WithNoValidatorsAndNoAnnotations_ShouldNotThrow()
    {
        // Arrange
        var sut = new StructuredDataValidator<TestModel>([]);

        // Act & Assert
        await Should.NotThrowAsync(() => sut.ValidateAndThrow(new TestModel()));
    }

    public class TestModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class AnnotatedModel
    {
        [Required]
        public string? RequiredField { get; set; }

        [StringLength(10)]
        public string? BoundedField { get; set; }
    }
}
