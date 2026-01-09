using Altinn.App.Core.Features;
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

    /// <summary>
    /// Adds a mechanism to map the datamodel of type <typeparamref name="TDataModel"/> to structured data of type <typeparamref name="TStructuredData"/>
    ///
    /// The data model is deleted right after PDF-generation so that it doesn't get transferred to storage (disable this behavior with <paramref name="keepAppDataModelAfterMapping"/>). The structured data will be stored instead.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="mapFunc">The function responsible for mapping from <typeparamref name="TDataModel"/> to <typeparamref name="TStructuredData"/> </param>
    /// <param name="includeErrorDetails">Whether to include error details in the structured data in case of mapping errors. Default is false.</param>
    /// <param name="keepAppDataModelAfterMapping">Whether to keep the App data model after mapping. Default is false.</param>
    /// <typeparam name="TStructuredData"></typeparam>
    /// <typeparam name="TDataModel"></typeparam>
    /// <returns></returns>
    /// <remarks>
    /// You need to add this to your App/config/applicationmetadata.json:
    /// <br/>
    /// {
    /// <br/>    "id": "structured-data",
    /// <br/>    "allowedContentTypes": [
    /// <br/>    "application/json"
    /// <br/>    ],
    /// <br/>    "allowedContributors": [
    /// <br/>    "app:owned"
    /// <br/>    ]
    /// <br/>},
    /// </remarks>
    public static IServiceCollection AddStructuredData<TDataModel, TStructuredData>(
        this IServiceCollection services,
        Func<TDataModel, TStructuredData> mapFunc,
        bool includeErrorDetails = false,
        bool keepAppDataModelAfterMapping = false
    )
        where TDataModel : class
        where TStructuredData : class
    {
        services.AddSingleton(
            new StructuredDataManager<TDataModel, TStructuredData>.Config(mapFunc)
            {
                IncludeErrorDetails = includeErrorDetails,
                DeleteAppDataModelAfterMapping = !keepAppDataModelAfterMapping,
            }
        );
        services.AddSingleton<StructuredDataManager<TDataModel, TStructuredData>>();
        services.AddTransient<IProcessEnd>(sp =>
            sp.GetRequiredService<StructuredDataManager<TDataModel, TStructuredData>>()
        );
        services.AddTransient<IProcessTaskEnd>(sp =>
            sp.GetRequiredService<StructuredDataManager<TDataModel, TStructuredData>>()
        );

        return services;
    }
}
