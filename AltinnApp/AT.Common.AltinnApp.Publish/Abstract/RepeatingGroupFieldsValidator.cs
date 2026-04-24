using System.Linq.Expressions;
using Altinn.App.Core.Features;
using Altinn.App.Core.Models.Validation;
using Altinn.Platform.Storage.Interface.Models;

namespace Arbeidstilsynet.Common.AltinnApp.Abstract;

/// <summary>
/// Base class for validators that focus on items within a collection in a data model.
/// Implementors provide an expression pointing to the collection and optionally an expression
/// pointing to a field within each item. <see cref="ValidateField"/> is called per item (or per
/// item's field when a field accessor is provided).
/// </summary>
/// <typeparam name="TDataModel">The root data model type</typeparam>
/// <typeparam name="TItem">The type of items in the collection</typeparam>
/// <typeparam name="TField">The type of the field to validate. Set to <typeparamref name="TItem"/> when no field accessor is used</typeparam>
public abstract class RepeatingGroupFieldsValidator<TDataModel, TItem, TField> : IFormDataValidator
    where TDataModel : class
{
    /// <summary>
    /// Return an expression pointing to the collection within the data model.
    /// </summary>
    protected abstract Expression<Func<TDataModel, IEnumerable<TItem>>> GetCollectionAccessor();

    /// <summary>
    /// Optionally return an expression pointing to a field within each collection item.
    /// When null, the entire item is passed to <see cref="ValidateField"/> (requires <typeparamref name="TField"/> = <typeparamref name="TItem"/>).
    /// </summary>
    protected virtual Expression<Func<TItem, TField>>? GetFieldAccessor() => null;

    /// <summary>
    /// Validate a single value. Called once per item in the collection.
    /// </summary>
    /// <param name="value">The field value (or entire item if no field accessor)</param>
    /// <param name="fieldPath">The indexed path, e.g. <c>Items[0].Name</c> or <c>Items[0]</c></param>
    protected abstract Task<List<ValidationIssue>> ValidateField(TField? value, string fieldPath);

    /// <inheritdoc />
    public virtual string DataType => "*";

    private Func<TDataModel, IEnumerable<TItem>>? _compiledCollectionAccessor;
    private Func<TItem, TField>? _compiledFieldAccessor;
    private string? _collectionPath;
    private string? _fieldPath;
    private bool _compiled;

    private void EnsureCompiled()
    {
        if (_compiled)
            return;

        var collectionExpr = GetCollectionAccessor();
        _collectionPath = StripParameterName(collectionExpr);
        _compiledCollectionAccessor = collectionExpr.Compile();

        var fieldExpr = GetFieldAccessor();
        if (fieldExpr != null)
        {
            _fieldPath = StripParameterName(fieldExpr);
            _compiledFieldAccessor = fieldExpr.Compile();
        }

        _compiled = true;
    }

    private static string StripParameterName<TIn, TOut>(Expression<Func<TIn, TOut>> expr)
    {
        var fullPath = expr.Body.ToString();
        var paramName = expr.Parameters[0].Name;
        return fullPath.StartsWith(paramName + ".")
            ? fullPath[(paramName!.Length + 1)..]
            : fullPath;
    }

    private IEnumerable<(TField? value, string path)> EnumerateFields(TDataModel dataModel)
    {
        EnsureCompiled();

        IEnumerable<TItem> collection;
        try
        {
            collection = _compiledCollectionAccessor!(dataModel) ?? [];
        }
        catch
        {
            yield break;
        }

        var index = 0;
        foreach (var item in collection)
        {
            var basePath = $"{_collectionPath}[{index}]";

            if (_compiledFieldAccessor != null)
            {
                TField? value;
                try
                {
                    value = _compiledFieldAccessor(item);
                }
                catch
                {
                    value = default;
                }

                yield return (value, $"{basePath}.{_fieldPath}");
            }
            else
            {
                TField? value;
                try
                {
                    value = (TField?)(object?)item;
                }
                catch
                {
                    value = default;
                }

                yield return (value, basePath);
            }

            index++;
        }
    }

    /// <inheritdoc />
    public bool HasRelevantChanges(object current, object previous)
    {
        if (current is not TDataModel currentModel || previous is not TDataModel previousModel)
        {
            return false;
        }

        var currentValues = EnumerateFields(currentModel).Select(x => x.value).ToList();
        var previousValues = EnumerateFields(previousModel).Select(x => x.value).ToList();

        return !currentValues.SequenceEqual(previousValues);
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
        {
            return [];
        }

        var validationResults = new List<ValidationIssue>();

        foreach (var (value, path) in EnumerateFields(dataModel))
        {
            var results = await ValidateField(value, path);
            validationResults.AddRange(results);
        }

        return validationResults;
    }
}
