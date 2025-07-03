using Arbeidstilsynet.Common.Altinn.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Arbeidstilsynet.Common.Altinn.DependencyInjection;


/// <summary>
/// Dependency injection extensions for Altinn
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
        services.AddSingleton<ILandskodeLookup, LandskodeLookup>();
        return services;
    }
}