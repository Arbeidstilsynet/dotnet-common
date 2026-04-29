using System.Linq.Expressions;
using Altinn.App.Core.Features;
using Altinn.App.Core.Models.Validation;
using Altinn.Platform.Storage.Interface.Models;

namespace Arbeidstilsynet.Common.AltinnApp.Abstract;

/// <summary>
/// Base class for validators that focus on items within a repeating group (collection) in a data model.
/// Implementors provide an expression pointing to the collection, and <see cref="ValidateItem"/> is called
/// once per item with the indexed path (e.g. <c>Items[0]</c>, <c>Nested.Tags[2]</c>).
/// </summary>
/// <typeparam name="TDataModel">The root data model type</typeparam>
/// <typeparam name="TItem">The type of items in the collection</typeparam>
public abstract class RepeatingGroupValidator<TDataModel, TItem> : IFormDataValidator
    where TDataModel : class
{
    /// <summary>
    /// Return an expression pointing to the collection within the data model.
    /// </summary>
    protected abstract Expression<Func<TDataModel, IEnumerable<TItem>>> GetCollectionAccessor();

    /// <summary>
    /// Validate a single item in the collection.
    /// </summary>
    /// <param name="item">The item</param>
    /// <param name="itemPath">The indexed path, e.g. <c>Items[0]</c>. Use as prefix for <see cref="ValidationIssue.Field"/></param>
    protected abstract Task<List<ValidationIssue>> ValidateItem(TItem? item, string itemPath);

    /// <inheritdoc />
    public virtual string DataType => "*";

    private Func<TDataModel, IEnumerable<TItem>>? _compiledCollectionAccessor;
    private string? _collectionPath;

    private void EnsureCompiled()
    {
        if (_compiledCollectionAccessor != null)
            return;

        var collectionExpr = GetCollectionAccessor();
        _collectionPath = StripParameterName(collectionExpr);
        _compiledCollectionAccessor = collectionExpr.Compile();
    }

    private static string StripParameterName<TIn, TOut>(Expression<Func<TIn, TOut>> expr)
    {
        var fullPath = expr.Body.ToString();
        var paramName = expr.Parameters[0].Name;
        return fullPath.StartsWith(paramName + ".")
            ? fullPath[(paramName!.Length + 1)..]
            : fullPath;
    }

    private IEnumerable<(TItem? item, string path)> EnumerateItems(TDataModel dataModel)
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
            yield return (item, $"{_collectionPath}[{index}]");
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

        var currentItems = EnumerateItems(currentModel).Select(x => x.item).ToList();
        var previousItems = EnumerateItems(previousModel).Select(x => x.item).ToList();

        return !currentItems.SequenceEqual(previousItems);
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

        foreach (var (item, path) in EnumerateItems(dataModel))
        {
            var results = await ValidateItem(item, path);
            validationResults.AddRange(results);
        }

        return validationResults;
    }
}
