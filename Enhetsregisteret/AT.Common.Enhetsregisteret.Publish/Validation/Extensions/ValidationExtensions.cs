using System.Text.RegularExpressions;

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

    [GeneratedRegex(
        @"^\d{9}$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant
    )]
    private static partial Regex OrgnummerRegex();
}
