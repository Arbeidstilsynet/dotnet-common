using Arbeidstilsynet.Common.Altinn.Storage.Models;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit.TestData;

/// <summary>
/// Helpers for building the generated storage models in tests.
/// </summary>
internal static class GeneratedModelExtensions
{
    /// <summary>
    /// Wraps a plain dictionary in the generated data-values type, whose entries live in an
    /// untyped <c>AdditionalData</c> bag because the specification models them as a free-form
    /// object.
    /// </summary>
    public static Instance_dataValues ToDataValues(this Dictionary<string, string>? dataValues)
    {
        return new Instance_dataValues
        {
            AdditionalData = (dataValues ?? []).ToDictionary(
                entry => entry.Key,
                entry => (object)entry.Value
            ),
        };
    }
}
