using Unleash;

namespace Arbeidstilsynet.Common.FeatureFlags.Model;

/// <summary>
/// Represents the context for feature flag evaluation.
/// Extends Unleash's UnleashContext to provide a typed model without requiring consumers to reference Unleash directly.
/// Inherits all properties from UnleashContext:
/// - UserId
/// - SessionId
/// - RemoteAddress
/// - Environment
/// - AppName
/// - CurrentTime
/// - Properties
/// </summary>
public class FeatureFlagContext : UnleashContext { }