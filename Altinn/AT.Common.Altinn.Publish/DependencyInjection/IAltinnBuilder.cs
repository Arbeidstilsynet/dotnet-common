using Arbeidstilsynet.Common.Altinn.Implementation.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arbeidstilsynet.Common.Altinn.DependencyInjection;

/// <summary>
/// Collects the Altinn clients and adapters an application needs.
/// </summary>
/// <remarks>
/// Obtained from <see cref="DependencyInjectionExtensions.AddAltinn"/>. Nothing that talks to an
/// Altinn API is registered until an <c>Add*</c> method is called on it, so an application only pays
/// for -- and only authenticates against -- the APIs it actually uses.
/// </remarks>
public interface IAltinnBuilder
{
    /// <summary>
    /// The service collection the clients are registered into.
    /// </summary>
    IServiceCollection Services { get; }
}

internal sealed class AltinnBuilder(
    IServiceCollection services,
    AltinnResolution resolution,
    AltinnOverrideRegistry overrides,
    IWebHostEnvironment hostEnvironment
) : IAltinnBuilder
{
    private readonly HashSet<string> _registered = [];

    public IServiceCollection Services { get; } = services;

    public AltinnResolution Resolution { get; } = resolution;

    public AltinnOverrideRegistry Overrides { get; } = overrides;

    public bool IsProductionHost { get; } = hostEnvironment.IsProduction();

    /// <summary>
    /// Claims a one-time registration slot, so adding the same client twice -- directly and via an
    /// adapter that depends on it -- registers its services only once.
    /// </summary>
    public bool TryRegister(string name) => _registered.Add(name);
}
