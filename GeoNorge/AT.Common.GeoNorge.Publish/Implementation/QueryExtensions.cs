using Arbeidstilsynet.Common.GeoNorge.Model.Request;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal static class QueryExtensions
{
    public static IReadOnlyDictionary<string, string> ToMap(this TextSearchQuery query)
    {
        var parameterMap = new Dictionary<string, string>();

        if (query.SearchTerm is { Length: > 0 } searchTerm)
        {
            parameterMap.Add("sok", searchTerm);
        }

        if (query.FuzzySearch)
        {
            parameterMap.Add("fuzzy", "true");
        }

        if (query.Adressenavn is { Length: > 0 } adressenavn)
        {
            parameterMap.Add("adressenavn", adressenavn);
        }

        if (query.Poststed is { Length: > 0 } poststed)
        {
            parameterMap.Add("poststed", poststed);
        }

        if (query.Postnummer is { Length: > 0 } postnummer)
        {
            parameterMap.Add("postnummer", postnummer);
        }

        if (query.Kommunenummer is { Length: > 0 } kommunenummer)
        {
            parameterMap.Add("kommunenummer", kommunenummer);
        }

        return parameterMap;
    }

    public static IReadOnlyDictionary<string, string> ToMap(this Pagination pagination)
    {
        var parameterMap = new Dictionary<string, string>();

        if (pagination.PageIndex >= 0)
        {
            parameterMap.Add(
                "side",
                pagination.PageIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)
            );
        }

        if (pagination.PageSize > 0)
        {
            parameterMap.Add(
                "treffPerSide",
                pagination.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture)
            );
        }

        return parameterMap;
    }

    public static IReadOnlyDictionary<string, string> ToMap(this PointSearchQuery query)
    {
        var parameterMap = new Dictionary<string, string>();

        parameterMap.Add(
            "lat",
            query.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)
        );
        parameterMap.Add(
            "lon",
            query.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)
        );
        parameterMap.Add(
            "radius",
            query.RadiusInMeters.ToString(System.Globalization.CultureInfo.InvariantCulture)
        );

        return parameterMap;
    }

    public static IReadOnlyDictionary<string, string> ToMap(this PointQuery query)
    {
        var parameterMap = new Dictionary<string, string>();
        
        parameterMap.Add("nord", 
            query.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture));
        
        parameterMap.Add("ost",
            query.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture));
        
        parameterMap.Add("koordsys", query.Epsg.ToString(System.Globalization.CultureInfo.InvariantCulture));
        
        return parameterMap;
    }
}
