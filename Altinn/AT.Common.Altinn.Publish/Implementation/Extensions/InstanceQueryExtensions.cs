using Arbeidstilsynet.Common.Altinn.Model.Api.Request;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Extensions;

internal static class InstanceQueryExtensions
{
    /// <summary>
    /// Carries the continuation token from a paged response's "next" link over to the next request.
    /// </summary>
    public static bool TryAppendContinuationToken(
        this InstanceQuery query,
        Uri uri,
        out InstanceQuery updatedQuery
    )
    {
        var queryParameters = uri
            .Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(param => param.Split('='))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1]);

        if (
            queryParameters.TryGetValue(
                InstanceQuery.ContinuationTokenParameterName,
                out var continuationToken
            )
        )
        {
            updatedQuery = query.WithContinuationToken(continuationToken);
            return true;
        }

        updatedQuery = query;

        return false;
    }
}
