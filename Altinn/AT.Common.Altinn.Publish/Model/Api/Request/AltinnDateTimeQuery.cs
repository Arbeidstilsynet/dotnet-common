namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

/// <summary>
/// Represents a date and comparison operator used in an Altinn query.
/// </summary>
public record AltinnDateTimeQuery
{
    /// <summary>
    /// Gets the comparison operator.
    /// </summary>
    public DateTimeCompareOperator CompareOperator { get; init; }

    /// <summary>
    /// Gets the date and time value to compare.
    /// </summary>
    public required string DateTime { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{CompareOperator}:{DateTime}";
    }
}

/// <summary>
/// Defines comparison operators for Altinn date and time queries.
/// </summary>
public enum DateTimeCompareOperator
{
    /// <summary>
    /// Greater than.
    /// </summary>
    gt,

    /// <summary>
    /// Greater than or equal to.
    /// </summary>
    gte,

    /// <summary>
    /// Less than.
    /// </summary>
    lt,

    /// <summary>
    /// Less than or equal to.
    /// </summary>
    lte,

    /// <summary>
    /// Equal to.
    /// </summary>
    eq,
}
