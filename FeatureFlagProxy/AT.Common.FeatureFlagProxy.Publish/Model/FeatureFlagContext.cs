using Unleash;

namespace Arbeidstilsynet.Common.FeatureFlag.Model;

/// <summary>
/// Represents the context for feature flag evaluation.
/// Extends Unleash's UnleashContext to provide a typed model without requiring consumers to reference Unleash directly.
/// </summary>
public class FeatureFlagContext : UnleashContext
{
  // Inherits all properties from UnleashContext:
  // - UserId
  // - SessionId
  // - RemoteAddress
  // - Environment
  // - AppName
  // - CurrentTime
  // - Properties
}
