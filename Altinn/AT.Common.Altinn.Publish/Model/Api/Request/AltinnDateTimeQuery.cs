namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

public record AltinnDateTimeQuery
{
    public DateTimeCompareOperator CompareOperator { get; init; }

    public required string DateTime { get; init; }

    public override string ToString()
    {
        return $"{CompareOperator}:{DateTime}";
    }
}

public enum DateTimeCompareOperator
{
    // greater than
    gt,

    // greater than or equal to
    gte,

    // less than
    lt,

    // less than or equal to
    lte,

    // equal
    eq,
}
