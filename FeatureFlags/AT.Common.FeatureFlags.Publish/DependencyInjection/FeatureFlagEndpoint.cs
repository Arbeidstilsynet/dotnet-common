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
        var group = endpoints.MapGroup(pattern).WithTags("Feature Flags");

        return group.MapPost(
            "/",
            (FeatureFlagRequest request, IFeatureFlags featureFlag) =>
            {
                bool isEnabled = featureFlag.IsEnabled(request.FeatureName, request.Context);
                return Results.Ok(new { IsEnabled = isEnabled });
            }
        );
    }
}

/// <summary>
/// Request model for feature flag check.
/// </summary>
public record FeatureFlagRequest
{
    /// <summary>
    /// The name of the feature flag to check.
    /// </summary>
    public required string FeatureName { get; init; }

    /// <summary>
    /// Optional context for feature flag evaluation.
    /// </summary>
    public FeatureFlagContext? Context { get; init; }
}
