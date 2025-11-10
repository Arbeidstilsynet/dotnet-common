using Arbeidstilsynet.Common.FeatureFlags.Model;
using Arbeidstilsynet.Common.FeatureFlags.Ports;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Arbeidstilsynet.Common.FeatureFlags.DependencyInjection;

/// <summary>
/// Extension methods for mapping the feature flag endpoint.
/// </summary>
public static class FeatureFlagEndpoint
{
    /// <summary>
    /// Maps the feature flag endpoint.
    /// </summary>
    /// <param name="endpoints"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder MapFeatureFlagEndpoint(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/featureflag"
    )
    {
        var group = endpoints.MapGroup(pattern).WithTags("FeatureFlags");

        return group.MapPost(
            "/",
            (FeatureFlagRequest request, IFeatureFlags featureFlag) =>
            {
                return new FeatureFlagResponse
                {
                    IsEnabled = featureFlag.IsEnabled(request.FeatureName, request.Context),
                    FeatureName = request.FeatureName,
                };
            }
        );
    }
}
