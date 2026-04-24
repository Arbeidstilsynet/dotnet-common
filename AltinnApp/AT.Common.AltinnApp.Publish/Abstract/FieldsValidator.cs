using System.Linq.Expressions;
using Altinn.App.Core.Features;
using Altinn.App.Core.Models.Validation;
using Altinn.Platform.Storage.Interface.Models;

namespace Arbeidstilsynet.Common.AltinnApp.Abstract;

/// <summary>
/// Base class for validators that focus on specific fields within a data model.
/// Implementors should provide expressions for the relevant fields and implement validation logic for individual field values.
/// The base class will handle change detection and enumeration of field values for validation.
/// </summary>
/// <typeparam name="TDataModel"></typeparam>
/// <typeparam name="TField"></typeparam>
public abstract class FieldsValidator<TDataModel, TField> : IFormDataValidator
    where TDataModel : class
{
    /// <summary>
    /// Return an expression for each field that should trigger validation when changed.
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerable<Expression<Func<TDataModel, TField>>> GetRelevantFields();

    /// <summary>
    /// Validate a single field value. The path parameter indicates the location of the field within the data model, which can be used for constructing validation issue paths.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="fieldPath">Put this in the <see cref="ValidationIssue.Field"/> of any <see cref="ValidationIssue"/></param>
    /// <returns></returns>
    protected abstract Task<List<ValidationIssue>> ValidateField(TField? value, string fieldPath);

    /// <inheritdoc />
    public virtual string DataType => "*";

    private Dictionary<string, Func<TDataModel, TField>>? _fieldAccessors;

    private IEnumerable<(TField? value, string path)> EnumerateFields(TDataModel dataModel)
    {
        _fieldAccessors ??= CompileAccessors();

        return _fieldAccessors.Select(kvp =>
            (ExpressionExtensions.DefensiveAccess(kvp.Value, dataModel), kvp.Key)
        );
    }

    private Dictionary<string, Func<TDataModel, TField>> CompileAccessors()
    {
        var dict = new Dictionary<string, Func<TDataModel, TField>>();
        foreach (var fieldExpr in GetRelevantFields())
        {
            if (fieldExpr.Body is not MemberExpression memberExpr)
            {
                throw new InvalidOperationException(
                    $"Expression '{fieldExpr}' is not a valid member expression."
                );
            }

            var fullPath =
                memberExpr.ToString()
                ?? throw new InvalidOperationException(
                    $"Could not determine path for expression '{fieldExpr}'."
                );

            // Strip the parameter name prefix (e.g. "m.Name" -> "Name", "x.Inner.Value" -> "Inner.Value")
            var paramName = fieldExpr.Parameters[0].Name;
            var path = fullPath.StartsWith(paramName + ".")
                ? fullPath[(paramName!.Length + 1)..]
                : fullPath;

            if (!dict.TryAdd(path, fieldExpr.Compile()))
            {
                throw new InvalidOperationException(
                    $"Duplicate field path '{path}' in expressions."
                );
            }
        }

        return dict;
    }

    /// <inheritdoc />
    public bool HasRelevantChanges(object current, object previous)
    {
        if (
            current is not TDataModel currentDataModel
            || previous is not TDataModel previousDataModel
        )
        {
            return false;
        }

        var currentValues = EnumerateFields(currentDataModel).Select(kvp => kvp.value);
        var previousValues = EnumerateFields(previousDataModel).Select(kvp => kvp.value);

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

file static class ExpressionExtensions
{
    public static TField? DefensiveAccess<TDataModel, TField>(
        this Func<TDataModel, TField> accessFunc,
        TDataModel dataModel
    )
    {
        try
        {
            return accessFunc(dataModel);
        }
        catch
        {
            return default;
        }
    }
}
