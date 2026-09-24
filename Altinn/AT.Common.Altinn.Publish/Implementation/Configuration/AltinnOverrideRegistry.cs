namespace Arbeidstilsynet.Common.Altinn.Implementation.Configuration;

/// <summary>
/// Accumulates the base URL overrides that actually took effect, from both
/// <see cref="DependencyInjection.AltinnUrlOverrides"/> and per-client configuration.
/// </summary>
/// <remarks>
/// Per-client overrides are applied while the service collection is still being built, after the
/// initial URL resolution has already happened, so they are collected here rather than on the
/// immutable resolution.
/// </remarks>
internal sealed class AltinnOverrideRegistry(IEnumerable<string> initialOverrides)
{
    private readonly List<string> _overrides = [.. initialOverrides];

    public void Add(string name) => _overrides.Add(name);

    public IReadOnlyList<string> Overrides => _overrides;
}
