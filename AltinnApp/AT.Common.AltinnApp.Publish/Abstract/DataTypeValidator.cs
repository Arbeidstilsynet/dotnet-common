using System.Collections;
using System.Reflection;
using Altinn.App.Core.Features;
using Altinn.App.Core.Models.Validation;
using Altinn.Platform.Storage.Interface.Models;

namespace Arbeidstilsynet.Common.AltinnApp.Abstract;

/// <summary>
/// Base class for validators that automatically discover and validate all instances of <typeparamref name="TToValidate"/>
/// within a data model by walking the object graph via reflection.
/// Handles nested objects and collections. Throws if the type graph contains circular references.
/// </summary>
/// <typeparam name="TDataModel">The root data model type</typeparam>
/// <typeparam name="TToValidate">The type to find and validate within the data model</typeparam>
public abstract class DataTypeValidator<TDataModel, TToValidate> : IFormDataValidator
    where TDataModel : class
    where TToValidate : class
{
    /// <summary>
    /// Validate a single discovered instance of <typeparamref name="TToValidate"/>.
    /// </summary>
    /// <param name="instance">The discovered instance</param>
    /// <param name="path">The dotted path to this instance within the data model, e.g. <c>Parent.Children[0].Address</c></param>
    protected abstract Task<List<ValidationIssue>> ValidateInstance(
        TToValidate instance,
        string path
    );

    /// <inheritdoc />
    public virtual string DataType => "*";

    private bool _typeGraphValidated;

    private void EnsureTypeGraphValidated()
    {
        if (_typeGraphValidated)
            return;

        ValidateTypeGraph(typeof(TDataModel), []);
        _typeGraphValidated = true;
    }

    private static void ValidateTypeGraph(Type type, HashSet<Type> visiting)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (IsLeafType(type) || type == typeof(TToValidate))
            return;

        var elementType = GetEnumerableElementType(type);
        if (elementType != null)
        {
            ValidateTypeGraph(elementType, visiting);
            return;
        }

        if (!visiting.Add(type))
        {
            throw new InvalidOperationException(
                $"Circular type reference detected: type '{type.FullName}' appears in its own object graph. "
                    + $"DataTypeValidator<{typeof(TDataModel).Name}, {typeof(TToValidate).Name}> cannot walk models with circular type references."
            );
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            ValidateTypeGraph(property.PropertyType, visiting);
        }

        visiting.Remove(type);
    }

    private static bool IsLeafType(Type type)
    {
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(Guid);
    }

    private static Type? GetEnumerableElementType(Type type)
    {
        if (type == typeof(string))
            return null;

        if (type.IsArray)
            return type.GetElementType();

        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return iface.GetGenericArguments()[0];
            }
        }

        return null;
    }

    private IEnumerable<(TToValidate instance, string path)> EnumerateInstances(
        TDataModel dataModel
    )
    {
        EnsureTypeGraphValidated();
        return WalkObject(dataModel, "", typeof(TDataModel));
    }

    private static IEnumerable<(TToValidate instance, string path)> WalkObject(
        object? obj,
        string currentPath,
        Type declaredType
    )
    {
        if (obj is null)
            yield break;

        if (obj is TToValidate target)
        {
            yield return (target, currentPath);
            yield break;
        }

        var type = Nullable.GetUnderlyingType(declaredType) ?? declaredType;

        if (IsLeafType(type))
            yield break;

        var elementType = GetEnumerableElementType(type);
        if (elementType != null && obj is IEnumerable enumerable)
        {
            var index = 0;
            foreach (var item in enumerable)
            {
                var itemPath = $"{currentPath}[{index}]";
                foreach (var result in WalkObject(item, itemPath, elementType))
                    yield return result;
                index++;
            }
            yield break;
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            object? value;
            try
            {
                value = property.GetValue(obj);
            }
            catch
            {
                continue;
            }

            var propPath = string.IsNullOrEmpty(currentPath)
                ? property.Name
                : $"{currentPath}.{property.Name}";

            foreach (var result in WalkObject(value, propPath, property.PropertyType))
                yield return result;
        }
    }

    /// <inheritdoc />
    public bool HasRelevantChanges(object current, object previous)
    {
        if (current is not TDataModel currentModel || previous is not TDataModel previousModel)
            return false;

        var currentInstances = EnumerateInstances(currentModel).Select(x => x.instance).ToList();
        var previousInstances = EnumerateInstances(previousModel).Select(x => x.instance).ToList();

        return !currentInstances.SequenceEqual(previousInstances);
    }

    /// <inheritdoc />
    public async Task<List<ValidationIssue>> ValidateFormData(
        Instance instance,
        DataElement dataElement,
        object data,
        string? language
    )
    {
        if (data is not TDataModel dataModel)
            return [];

        var validationResults = new List<ValidationIssue>();

        foreach (var (target, path) in EnumerateInstances(dataModel))
        {
            var results = await ValidateInstance(target, path);
            validationResults.AddRange(results);
        }

        return validationResults;
    }
}
