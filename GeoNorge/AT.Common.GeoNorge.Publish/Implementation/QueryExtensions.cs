using System.Text;
using Arbeidstilsynet.Common.GeoNorge.Model.Request;
using Microsoft.Extensions.Primitives;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal static class QueryExtensions
{
    public static Uri AddQueryParameters(
        this Uri uri,
        IReadOnlyDictionary<string, string> parameterMap
    )
    {
        if (parameterMap.Count == 0)
        {
            return uri;
        }

        var sb = new StringBuilder();

        sb.Append(uri);
        
        if (uri.ToString().Contains('?'))
        {
            sb.Append('&');
        }
        else
        {
            sb.Append('?');
        }

        sb.Append(parameterMap.ToQueryParameters());

        var paremeterizedUri = sb.ToString();
        
        return new Uri(paremeterizedUri, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
    }

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
        
        if (query.Kommunenummer is {Length: > 0 } kommunenummer)
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
            parameterMap.Add("side", pagination.PageIndex.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        
        if (pagination.PageSize > 0)
        {
            parameterMap.Add("treffPerSide", pagination.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return parameterMap;
    }

    public static IReadOnlyDictionary<string, string> ToMap(this PointSearchQuery query)
    {
        var parameterMap = new Dictionary<string, string>();

        parameterMap.Add("lat", query.Latitude.ToFormattedString());
        parameterMap.Add("lon", query.Longitude.ToFormattedString());
        parameterMap.Add("radius", query.RadiusInMeters.ToString(System.Globalization.CultureInfo.InvariantCulture));

        return parameterMap;
    }
    
    private static string ToFormattedString(this double value)
    {
        return value.ToString("F6", System.Globalization.CultureInfo.InvariantCulture).Trim('0');
    }

    private static string ToQueryParameters(this IReadOnlyDictionary<string, string> parameterMap)
    {
        var sb = new StringBuilder();

        foreach (var kvp in parameterMap)
        {
            if (sb.Length > 0)
            {
                sb.Append('&');
            }
            sb.Append($"{kvp.Key}={kvp.Value}");
        }
        return sb.ToString();
    }
}