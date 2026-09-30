using Arbeidstilsynet.Common.Altinn.Model.Api.Request;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Extensions;

internal static class InstanceQueryParametersExtensions
{
    /// <summary>
    /// Carries the continuation token from a paged response's "next" link over to the next request.
    /// </summary>
    public static bool TryAppendContinuationToken(
        this InstanceQueryParameters instanceQueryParameters,
        Uri uri,
        out InstanceQueryParameters updatedQueryParameters
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
                InstanceQueryParameters.ContinuationTokenParameterName,
                out var continuationToken
            )
        )
        {
            updatedQueryParameters = instanceQueryParameters with
            {
                ContinuationToken = continuationToken,
            };
            return true;
        }

        updatedQueryParameters = instanceQueryParameters;

        return false;
    }
}
