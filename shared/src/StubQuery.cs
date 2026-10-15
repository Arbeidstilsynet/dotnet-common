namespace Arbeidstilsynet.Shared.Implementation;

internal static class StubQuery
{
    public static void Validate(object query, params string[] supportedProperties)
    {
        ArgumentNullException.ThrowIfNull(query);
        foreach (var property in query.GetType().GetProperties())
        {
            if (
                property.GetValue(query) is not null
                && !supportedProperties.Contains(property.Name)
            )
            {
                throw new NotSupportedException(
                    $"The Development stub does not support '{property.Name}'. Configure an API endpoint or replace the interface implementation."
                );
            }
        }
    }

    public static void GeographicCoordinates(int? coordinateSystem)
    {
        if (coordinateSystem is not null and not 4326)
        {
            throw new NotSupportedException(
                "Development stubs only support EPSG:4326 coordinates."
            );
        }
    }

    public static void Page(double? page, double? size)
    {
        if (page is { } p && (!double.IsFinite(p) || p < 0 || p != Math.Truncate(p)))
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                "Page must be a non-negative integer."
            );
        }
        if (size is { } s && (!double.IsFinite(s) || s <= 0 || s != Math.Truncate(s)))
        {
            throw new ArgumentOutOfRangeException(
                nameof(size),
                "Page size must be a positive integer."
            );
        }
    }
}
