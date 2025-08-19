using System.Text;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;

/// <summary>
/// Extensions for working with <see cref="Uri"/> objects.
/// </summary>
public static class UriExtensions
{
    /// <summary>
    /// Adds query parameters to a URI. Query parameters are added like "?key1=value1&amp;key2=value2" (or leading with '&amp;' if <paramref name="uri"/> contains parameters.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to base the resulting <see cref="Uri"/> on</param>
    /// <param name="queryParameters">Query parameters as key value pairs. Pairs with blank values are ignored.</param>
    /// <returns>A new <see cref="Uri"/> with the query parameters added. If <paramref name="queryParameters"/> is empty, the same Uri is returned instead.</returns>
    /// <remarks>
    /// Values are URL-escaped.
    /// <br/>
    /// No effort is made to ensure that the query parameters are unique or that keys do not conflict with existing query parameters.
    /// </remarks>
    public static Uri AddQueryParameters(
        this Uri uri,
        IEnumerable<KeyValuePair<string, string>> queryParameters
    )
    {
        var validQueryParameters = queryParameters
            .Where(kvPair => !string.IsNullOrWhiteSpace(kvPair.Value))
            .ToList();

        if (validQueryParameters.Count == 0)
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

        sb.Append(validQueryParameters.ToQueryParameters());

        var parameterizedUri = sb.ToString();

        return new Uri(parameterizedUri, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
    }

    private static string ToQueryParameters(
        this IEnumerable<KeyValuePair<string, string>> parameterMap
    )
    {
        var sb = new StringBuilder();

        foreach (var (key, value) in parameterMap)
        {
            if (sb.Length > 0)
            {
                sb.Append('&');
            }
            var escapedValue = Uri.EscapeDataString(value);

            sb.Append($"{key}={escapedValue}");
        }
        return sb.ToString();
    }
}
