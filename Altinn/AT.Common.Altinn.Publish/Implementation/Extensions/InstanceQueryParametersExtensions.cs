using System.Reflection;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;

namespace Arbeidstilsynet.Common.Altinn.Implementation;

internal static class InstanceQueryParametersExtensions
{
    public static bool TryAppendContinuationToken(this InstanceQueryParameters instanceQueryParameters, Uri uri, out InstanceQueryParameters updatedQueryParameters)
    {
        var queryParameters = uri.Query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(param => param.Split('='))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1]);
        
        if (queryParameters.TryGetValue(InstanceQueryParameters.ContinuationTokenParameterName, out var continuationToken))
        {
            updatedQueryParameters = instanceQueryParameters with
            {
                ContinuationToken = continuationToken
            };
            return true;
        }
        updatedQueryParameters = instanceQueryParameters;
        
        return false;
    }
    
    public static IEnumerable<(string, string)> GetQueryParameters(
        this InstanceQueryParameters? queryParameters
    )
    {
        if (queryParameters == null)
        {
            yield break;
        }

        var selectedParameters = queryParameters
            .GetType()
            .GetProperties()
            .Select(p => (p.GetQueryParameterName(), p.GetValue(queryParameters)));
        
        foreach (var (queryParameterName, parameterValue) in selectedParameters)
        {
            if (parameterValue is null || queryParameterName is null)
            {
                continue;
            }
            
            if (parameterValue is ICollection<object> parameterCollection)
            {
                foreach (var itemValue in parameterCollection)
                {
                    yield return (queryParameterName, itemValue.ToString()!);
                }
            }
            else
            {
                yield return (queryParameterName, parameterValue.ToString()!);
            }
        }
    }
    
    public static IEnumerable<(string, string)> GetHeaderParameters(
        this InstanceQueryParameters? queryParameters
    )
    {
        if (queryParameters == null)
        {
            yield break;
        }

        var selectedParameters = queryParameters
            .GetType()
            .GetProperties()
            .Select(p => (p.GetHeaderParameterName(), p.GetValue(queryParameters)));
        
        foreach (var (headerParameterName, parameterValue) in selectedParameters)
        {
            if (parameterValue is null || headerParameterName is null)
            {
                continue;
            }

            yield return (headerParameterName, parameterValue.ToString()!);
        }
    }
    
    private static string? GetQueryParameterName(
        this PropertyInfo propertyInfo
    )
    {
        if (propertyInfo.GetCustomAttribute<MappedQueryParameterAttribute>() is not {QueryParameterName: { Length: > 0 } queryParameterName})
        {
            return null;
        }
        
        return queryParameterName;
    }
    
    private static string? GetHeaderParameterName(
        this PropertyInfo propertyInfo
    )
    {
        if (propertyInfo.GetCustomAttribute<MappedRequestHeaderParameterAttribute>() is not {HeaderParameterName: { Length: > 0 } headerParameterName})
        {
            return null;
        }
        
        return headerParameterName;
    }
    
}