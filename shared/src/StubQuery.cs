namespace Arbeidstilsynet.Shared.Implementation;

internal static class StubQuery
{
    public static void Validate(object query, params string[] supportedProperties)
    {
        ArgumentNullException.ThrowIfNull(query);
        var unsupportedProperty = query
            .GetType()
            .GetProperties()
            .FirstOrDefault(property =>
                property.GetValue(query) is not null && !supportedProperties.Contains(property.Name)
            );
        if (unsupportedProperty is not null)
        {
            throw new NotSupportedException(
                $"The Development stub does not support '{unsupportedProperty.Name}'. Configure an API endpoint or replace the interface implementation."
            );
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
        if (page is { } p && (!double.IsInteger(p) || p < 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                "Page must be a non-negative integer."
            );
        }
        if (size is { } s && (!double.IsInteger(s) || s <= 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(size),
                "Page size must be a positive integer."
            );
        }
    }
}
