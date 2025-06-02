using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions;

/// <summary>
/// Extensions of the IConfiguration abstraction
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Tries to get an object of type T from the top level of the configuration.
    /// The object is validated using the System.ComponentModel.DataAnnotations attributes.
    ///
    /// Throws an exception if the object cannot be found in the configuration, or if validation of the object fails.
    /// </summary>
    /// <param name="configuration"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static T GetRequired<T>(this IConfiguration configuration)
    {
        var config =
            configuration.Get<T>()
            ?? throw new InvalidOperationException(
                $"Missing configuration for `{typeof(T).Name}` which is required."
            );

        config.EnsureValid();

        return config;
    }

    private static bool TryValidateRecursive(
        this object? section,
        out Dictionary<string, List<ValidationResult>> validationResults,
        string sectionPath = "",
        HashSet<object>? traversedSections = null
    )
    {
        validationResults = [];
        traversedSections ??= [];

        if (section is null)
        {
            return true;
        }

        var validationContext = new ValidationContext(section);
        var sectionValidationResults = new List<ValidationResult>();
        if (
            !Validator.TryValidateObject(section, validationContext, sectionValidationResults, true)
        )
        {
            var cleanedResults = sectionValidationResults
                .Select(v => v.ScrubErrorMessage(section))
                .ToList();

            validationResults.Add(sectionPath, cleanedResults);
        }

        foreach (var property in section.GetComplexProperties())
        {
            var value = property.GetValue(section);
            var name = sectionPath.AppendPathPart(property.GetPropertyName());

            if (
                value is null // Terminate
                || !traversedSections.Add(value) // Prevent infinite recursion
                || value.TryValidateRecursive(
                    out var innerValidationResults,
                    name,
                    traversedSections
                ) // Validate
            )
            {
                continue;
            }

            validationResults.Merge(innerValidationResults);
        }

        return !validationResults.Values.SelectMany(v => v).Any();
    }

    private static void EnsureValid<T>(this T config)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        if (config.TryValidateRecursive(out var validationResults))
            return;

        var errorBuilder = new StringBuilder();

        foreach (var (key, results) in validationResults)
        {
            if (results.Count == 0)
            {
                continue;
            }

            errorBuilder.AppendLine($"Section: {key}");
            foreach (var result in results)
            {
                errorBuilder.AppendLine($" - {result.ErrorMessage}");
            }
        }

        throw new InvalidOperationException(
            $"Invalid configuration for `{typeof(T).Name}`: {Environment.NewLine}{errorBuilder}"
        );
    }

    private static ValidationResult ScrubErrorMessage(this ValidationResult result, object section)
    {
        if (result.ErrorMessage == null)
        {
            return result;
        }

        var scrubbedErrorMessage = result.ErrorMessage;

        foreach (var propertyInfo in section.GetType().GetProperties())
        {
            var name = propertyInfo.Name;
            var keyName = propertyInfo.GetPropertyName();

            if (name == keyName) // No need to scrub
            {
                continue;
            }

            var regex = new Regex($@"\b{name}\b", RegexOptions.None, TimeSpan.FromSeconds(10));

            scrubbedErrorMessage = regex.Replace(scrubbedErrorMessage, keyName);
        }

        return new ValidationResult(scrubbedErrorMessage, result.MemberNames);
    }

    private static void Merge(
        this Dictionary<string, List<ValidationResult>> target,
        Dictionary<string, List<ValidationResult>> source
    )
    {
        foreach (var (key, sourceValue) in source)
        {
            if (!target.TryGetValue(key, out var targetResults))
            {
                target[key] = sourceValue.ToList();
            }
            else
            {
                targetResults.AddRange(sourceValue);
            }
        }
    }

    private static string AppendPathPart(this string? path, string name)
    {
        if (path is not { Length: > 0 })
        {
            return name;
        }

        return $"{path}:{name}";
    }

    private static string GetPropertyName(this PropertyInfo property)
    {
        return property.GetCustomAttribute<ConfigurationKeyNameAttribute>()?.Name ?? property.Name;
    }

    private static IEnumerable<PropertyInfo> GetComplexProperties(this object obj)
    {
        return obj.GetType().GetProperties().Where(p => p.PropertyType.IsComplex());
    }

    private static bool IsComplex(this Type type)
    {
        if (type == typeof(string))
            return false;
        if (type.IsPrimitive)
            return false;
        if (type.IsEnum)
            return false;
        if (type == typeof(decimal))
            return false;
        if (type == typeof(DateTime))
            return false;
        if (type == typeof(DateTimeOffset))
            return false;
        if (type == typeof(TimeSpan))
            return false;
        if (type == typeof(Guid))
            return false;
        if (type == typeof(Uri))
            return false;

        return type.IsClass || type is { IsValueType: true, IsPrimitive: false };
    }
}
