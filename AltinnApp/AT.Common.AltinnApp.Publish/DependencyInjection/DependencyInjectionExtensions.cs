using Arbeidstilsynet.Common.AltinnApp.Implementation;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.AltinnApp.DependencyInjection;

/// <summary>
/// Configuration for the LandOptions feature.
/// </summary>
/// <param name="OptionsId">The Altinn optionsId, default is "land".</param>
public record LandOptionsConfiguration(string OptionsId = "land");

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
