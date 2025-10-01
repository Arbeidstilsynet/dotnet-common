using System.Text.RegularExpressions;
using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using FluentValidation;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Validation.Extensions;

internal static partial class ValidationExtensions
{
    private static readonly Regex OrganisasjonsnummerRegex = OrgnummerRegex();

    public static void ValidateOrgnummerOrThrow(this string? orgnummer, string paramName)
    {
        if (!orgnummer.IsValidOrgnummer())
        {
            throw new ArgumentException($"Invalid organisasjonsnummer: {orgnummer}", paramName);
        }
    }

    public static bool IsValidOrgnummer(this string? orgnummer)
    {
        return !string.IsNullOrWhiteSpace(orgnummer) && OrganisasjonsnummerRegex.IsMatch(orgnummer);
    }

    public static void ValidateAndThrow<T>(this IEnumerable<IValidator> validators, T instance)
    {
        if (!validators.TryValidate(instance, out var errors))
        {
            throw new ArgumentException(string.Join("; ", errors));
        }
    }

    private static bool TryValidate<T>(
        this IEnumerable<IValidator> validators,
        T instance,
        out List<string> errors
    )
    {
        var validationErrors = new List<string>();
        foreach (var validator in validators.Where(v => v.CanValidateInstancesOfType(typeof(T))))
        {
            var validationContext = new ValidationContext<T>(instance);
            var result = validator.Validate(validationContext);
            if (!result.IsValid)
            {
                validationErrors.AddRange(result.Errors.Select(e => e.ErrorMessage));
            }
        }

        errors = validationErrors;
        return errors.Count == 0;
    }

    [GeneratedRegex(
        @"^\d{9}$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant
    )]
    private static partial Regex OrgnummerRegex();
}
