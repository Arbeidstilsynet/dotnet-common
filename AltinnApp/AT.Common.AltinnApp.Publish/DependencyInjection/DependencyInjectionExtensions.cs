using Arbeidstilsynet.Common.AltinnApp.Implementation;
using Arbeidstilsynet.Common.AltinnApp.Model;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.AltinnApp.DependencyInjection;

/// <summary>
/// Configuration for the LandOptions feature.
/// </summary>
public record LandOptionsConfiguration
{
    /// <summary>
    /// Selector for which code to use as the option value.
    /// </summary>
    public enum IsoType
    {
        /// <summary>
        /// ISO 3166-1 alpha-3, e.g. "NOR" for Norway.
        /// </summary>
        Alpha3,

        /// <summary>
        /// ISO 3166-1 alpha-2, e.g. "NO" for Norway.
        /// </summary>
        Alpha2,
    }

    /// <summary>
    /// The Altinn optionsId, default is "land".
    /// </summary>
    public string OptionsId { get; init; } = "land";

    /// <summary>
    /// Custom ordering function for the list of countries. Default is alphabetical order.
    /// </summary>
    public Func<IEnumerable<Landskode>, IEnumerable<Landskode>>? CustomOrderFunc { get; init; }

    /// <summary>
    /// Which ISO type to use for the option value. Default is Alpha3.
    /// </summary>
    public IsoType OptionValueIsoType { get; init; } = IsoType.Alpha3;
}

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds a <see cref="ILandskodeLookup"/> to look up countries and their dial codes based on 3-letter ISO values.
    /// </summary>
    /// <param name="services"></param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddLandskoder(this IServiceCollection services)
    {
        services.TryAddSingleton<ILandskodeLookup, LandskodeLookup>();
        return services;
    }

    /// <summary>
    /// Adds the LandOptions feature to the service collection. This also adds <see cref="ILandskodeLookup"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="optionsConfiguration"></param>
    /// <returns></returns>
    public static IServiceCollection AddLandOptions(
        this IServiceCollection services,
        LandOptionsConfiguration? optionsConfiguration = null
    )
    {
        optionsConfiguration ??= new LandOptionsConfiguration();

        services.AddLandskoder();

        services.TryAddSingleton(Options.Create(optionsConfiguration));
        services.TryAddSingleton<Altinn.App.Core.Features.IAppOptionsProvider, LandOptions>();

        return services;
    }
}
