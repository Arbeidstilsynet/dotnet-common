namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

public class AltinnDateTimeQuery
{
    public DateTimeCompareOperator CompareOperator { get; set; }

    public string DateTime { get; set; }

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
