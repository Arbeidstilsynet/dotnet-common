using FluentValidation;
using FluentValidation.Results;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal interface IStructuredDataValidator<in T>
    where T : class
{
    Task ValidateAndThrow(T structuredData);
}

internal class StructuredDataValidator<T>(IEnumerable<IValidator<T>> validators) : IStructuredDataValidator<T>
    where T : class
{
    public async Task ValidateAndThrow(T structuredData)
    {
        List<ValidationFailure> failures;

        if (validators.Any())
        {
            failures = [];
            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(structuredData);
                failures.AddRange(result.Errors);
            }
        }
        else
        {
            var validationResults =
                new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var validationContext =
                new System.ComponentModel.DataAnnotations.ValidationContext(structuredData);
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                structuredData,
                validationContext,
                validationResults,
                validateAllProperties: true
            );
            failures = validationResults
                .Select(r => new ValidationFailure(
                    string.Join(", ", r.MemberNames),
                    r.ErrorMessage
                ))
                .ToList();
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }
    }
}