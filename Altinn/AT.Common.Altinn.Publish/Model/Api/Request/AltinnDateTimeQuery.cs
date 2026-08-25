namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

/// <summary>
/// Builds the date comparison expressions Altinn's instance query expects, such as
/// <c>gte:2024-01-01</c>.
/// </summary>
/// <remarks>
/// The storage specification types these parameters as plain strings, so the generated query
/// parameters take a <see cref="string"/> array. These helpers exist so callers do not have to
/// hand-write the prefix syntax.
/// </remarks>
public static class AltinnDateTimeQuery
{
    /// <summary>Matches instants strictly after <paramref name="dateTime"/>.</summary>
    public static string GreaterThan(DateTimeOffset dateTime) => Format("gt", dateTime);

    /// <summary>Matches instants at or after <paramref name="dateTime"/>.</summary>
    public static string GreaterThanOrEquals(DateTimeOffset dateTime) => Format("gte", dateTime);

    /// <summary>Matches instants strictly before <paramref name="dateTime"/>.</summary>
    public static string LessThan(DateTimeOffset dateTime) => Format("lt", dateTime);

    /// <summary>Matches instants at or before <paramref name="dateTime"/>.</summary>
    public static string LessThanOrEquals(DateTimeOffset dateTime) => Format("lte", dateTime);

    /// <summary>Matches instants equal to <paramref name="dateTime"/>.</summary>
    public static string Equals(DateTimeOffset dateTime) => Format("eq", dateTime);

    private static string Format(string comparisonOperator, DateTimeOffset dateTime) =>
        $"{comparisonOperator}:{dateTime:O}";
}
