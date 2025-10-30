namespace Arbeidstilsynet.Common.FeatureFlags.Model;

/// <summary>
/// DTOs should be under the *.Model namespace.
/// </summary>
public record FeatureFlagsDto
{
    /// <summary>
    /// Required summary for public property.
    /// </summary>
    public required string Foo { get; init; }
}
