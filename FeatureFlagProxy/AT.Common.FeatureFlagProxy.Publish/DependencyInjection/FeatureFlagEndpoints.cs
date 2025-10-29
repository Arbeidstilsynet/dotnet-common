using Arbeidstilsynet.Common.FeatureFlag.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Arbeidstilsynet.Common.FeatureFlag.DependencyInjection;

/// <summary>
/// Extensions for mapping feature flag endpoints.
/// </summary>
public static class FeatureFlagEndpoints
{
  /// <summary>
  /// Maps a POST endpoint for checking if a feature flag is enabled.
  /// </summary>
  /// <param name="endpoints">The endpoint route builder.</param>
  /// <param name="pattern">The route pattern (e.g., "/featureflag").</param>
  /// <returns>The route handler builder for further configuration.</returns>
  public static RouteHandlerBuilder MapFeatureFlagEndpoint(
      this IEndpointRouteBuilder endpoints,
      string pattern = "/featureflag"
  )
  {
    return endpoints.MapPost(
        pattern,
        (FeatureFlagRequest request, IFeatureFlag featureFlag) =>
        {
          var isEnabled = featureFlag.IsEnabled(request.FeatureName, request.Context);
          return Results.Ok(new FeatureFlagResponse
          {
            FeatureName = request.FeatureName,
            IsEnabled = isEnabled
          });
        }
    ).WithName("CheckFeatureFlag");
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

/// <summary>
/// Response model for feature flag check.
/// </summary>
public record FeatureFlagResponse
{
  /// <summary>
  /// The name of the feature flag that was checked.
  /// </summary>
  public required string FeatureName { get; init; }

  /// <summary>
  /// Whether the feature flag is enabled.
  /// </summary>
  public required bool IsEnabled { get; init; }
}
