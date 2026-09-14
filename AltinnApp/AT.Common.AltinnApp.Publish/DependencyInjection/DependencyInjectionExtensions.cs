using Altinn.ApiClients.Dialogporten.ServiceOwner;
using Altinn.App.Api.Extensions;
using Altinn.App.Core.Features;
using Altinn.App.Core.Internal.Process.ProcessTasks.ServiceTasks;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Implementation;
using Arbeidstilsynet.Common.AltinnApp.Model;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
/// Configuration for structured data mapping.
/// </summary>
public record StructuredDataConfiguration
{
    /// <summary>
    /// The <see cref="DataElement.DataType"/> of the structured data (if any) in <see cref="Instance.Data"/>. Defaults to "structured-data".
    /// </summary>
    /// <remarks>This id default for Arbeidstilsynet and should be unnecessary to override.</remarks>
    public string StructuredDataTypeId { get; init; } = "structured-data";

    /// <summary>
    /// The <see cref="DataElement.DataType"/> of the main PDF document in <see cref="Instance.Data"/>. Defaults to "ref-data-as-pdf".
    /// </summary>
    /// <remarks>This is default for Altinn Apps and should basically never be overridden.</remarks>
    public string MainPdfDataTypeId { get; init; } = "ref-data-as-pdf";

    /// <summary>
    /// Whether to include error details in the structured data in case of mapping errors. Default is false.
    /// </summary>
    public bool IncludeErrorDetails { get; init; } = false;

    /// <summary>
    /// Whether to disable validation of the structured data against the JSON schema during mapping. Default is false.
    /// </summary>
    public bool DisableValidation { get; init; } = false;

    /// <summary>
    /// Whether to keep the App data model after mapping. Default is false.
    /// </summary>
    public bool KeepAppDataModelAfterMapping { get; init; } = false;

    /// <summary>
    /// Optional filter for which task(s) the structured data mapping should run for. Default is empty, which means it will run for all tasks.
    /// </summary>
    /// <remarks>
    /// If this filter is set and excludes the submission task, no structured data will be generated for that task,
    /// which means the post-submission handler will fail because structured data is expected to exist by process end.
    /// Ensure the submission task (usually Task_1) is included in this filter.
    /// </remarks>
    public string[] TaskIdFilter { get; init; } = [];
}

/// <summary>
/// Extensions for Dependency Injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds a singleton language observer of type <typeparamref name="T"/> to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddLanguageObserver<T>(this IServiceCollection services)
        where T : class, ILanguageObserver
    {
        services.TryAddTransient<IDataProcessor, SelectedLanguageProcessor>();
        services.AddSingleton<ILanguageObserver, T>();
        return services;
    }

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
    /// The data model is deleted right after PDF-generation so that it doesn't get transferred to storage (control this behavior with <see cref="StructuredDataConfiguration.KeepAppDataModelAfterMapping"/>). The structured data will be stored instead.
    /// A structured data element is always expected to exist by process end, regardless of whether the data model is kept.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="mapFunc">The function responsible for mapping from <typeparamref name="TDataModel"/> to <typeparamref name="TStructuredData"/>.</param>
    /// <param name="configuration">The structured data configuration.</param>
    /// <typeparam name="TStructuredData">The type of the structured data that the App data model will be mapped to and stored as.</typeparam>
    /// <typeparam name="TDataModel">The type of the App data model. This is the type that will be mapped to structured data and then deleted before storage.</typeparam>
    public static IServiceCollection AddStructuredData<TDataModel, TStructuredData>(
        this IServiceCollection services,
        Func<TDataModel, TStructuredData> mapFunc,
        StructuredDataConfiguration configuration
    )
        where TDataModel : class
        where TStructuredData : class
    {
        var config = new StructuredDataManager<TDataModel, TStructuredData>.Config(mapFunc)
        {
            StructuredDataConfiguration = configuration,
        };

        return services.AddStructuredDataInternal(config);
    }

    /// <summary>
    /// Adds a mechanism to map the datamodel of type <typeparamref name="TDataModel"/> to structured data of type <typeparamref name="TStructuredData"/>
    /// <br/>
    /// The data model is deleted right after PDF-generation so that it doesn't get transferred to storage (control this behavior with <see cref="StructuredDataConfiguration.KeepAppDataModelAfterMapping"/>). The structured data will be stored instead.
    /// <br/>
    /// A structured data element is always expected to exist by process end, regardless of whether the data model is kept.
    /// <br/>
    /// The app instance will also declare the <see cref="DataElement.DataType"/> of the structured data and main content. This declaration is written to DataValues, based on the <see cref="StructuredDataConfiguration"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="mapFunc">The function responsible for mapping from <typeparamref name="TDataModel"/> to <typeparamref name="TStructuredData"/>.</param>
    /// <param name="env">The hosting environment. Used to determine which defaults to use in the configuration. In production, error details will be excluded from the structured data by default, while in non-production environments, error details will be included by default.</param>
    /// <typeparam name="TStructuredData">The type of the structured data that the App data model will be mapped to and stored as.</typeparam>
    /// <typeparam name="TDataModel">The type of the App data model. This is the type that will be mapped to structured data and then deleted before storage.</typeparam>
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
        IHostEnvironment env
    )
        where TDataModel : class
        where TStructuredData : class
    {
        var config = new StructuredDataManager<TDataModel, TStructuredData>.Config(mapFunc)
        {
            StructuredDataConfiguration = new StructuredDataConfiguration()
            {
                IncludeErrorDetails = !env.IsProduction(),
            },
        };

        return services.AddStructuredDataInternal(config);
    }

    private static IServiceCollection AddStructuredDataInternal<TDataModel, TStructuredData>(
        this IServiceCollection services,
        StructuredDataManager<TDataModel, TStructuredData>.Config config
    )
        where TDataModel : class
        where TStructuredData : class
    {
        services.AddSingleton(config);
        services.AddTransient<StructuredDataManager<TDataModel, TStructuredData>>();
        services.AddTransient<
            IStructuredDataValidator<TStructuredData>,
            StructuredDataValidator<TStructuredData>
        >();
        services.AddTransient<IProcessEnd>(sp =>
            sp.GetRequiredService<StructuredDataManager<TDataModel, TStructuredData>>()
        );
        services.AddTransient<IProcessTaskEnd>(sp =>
            sp.GetRequiredService<StructuredDataManager<TDataModel, TStructuredData>>()
        );

        return services;
    }

    /// <summary>
    /// Registers the <see cref="PatchDialogTask{TDataModel}"/> service task along with its
    /// required dependencies (Dialogporten client, patch operations provider and organisasjonsnummer provider).
    /// </summary>
    /// <typeparam name="TDataModel">The skjema data model type related to the instance.</typeparam>
    /// <typeparam name="TOrgNrProvider">The organisasjonsnummer provider type.</typeparam>
    /// <typeparam name="TPatchOperationsProvider">The patch operations provider type.</typeparam>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="env">The host environment, used to select production or test Dialogporten endpoints.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddPatchDialogTask<
        TDataModel,
        TOrgNrProvider,
        TPatchOperationsProvider
    >(this IServiceCollection services, IHostEnvironment env)
        where TDataModel : class
        where TOrgNrProvider : class, ISubmittersOrganisasjonsnummerProvider<TDataModel>
        where TPatchOperationsProvider : class, IPatchOperationsProvider
    {
        services.AddDialogportenClientForEnvironment(env);
        return services
            .AddTransient<ISubmittersOrganisasjonsnummerProvider<TDataModel>, TOrgNrProvider>()
            .AddTransient<IPatchOperationsProvider, TPatchOperationsProvider>()
            .AddTransient<IServiceTask, PatchDialogTask<TDataModel>>();
    }

    /// <summary>
    /// Registers the <see cref="UpdateDialogTask{TDataModel}"/> service task along with its
    /// required dependencies (Dialogporten client, patch operations provider, update dialog provider
    /// and organisasjonsnummer provider).
    /// </summary>
    /// <remarks>
    /// Use this task for handling updates (endringsmelding) of a melding. In contrast to
    /// the patch dialog task, the update may relate to a melding that was
    /// not received via Altinn. In that case the <see cref="IUpdateDialogProvider{T}"/> instructs the
    /// task to create a new dialog; otherwise the existing Altinn dialog is reused.
    /// </remarks>
    /// <typeparam name="TDataModel">The skjema data model type related to the instance.</typeparam>
    /// <typeparam name="TOrgNrProvider">The organisasjonsnummer provider type.</typeparam>
    /// <typeparam name="TPatchOperationsProvider">The patch operations provider type.</typeparam>
    /// <typeparam name="TUpdateDialogProvider">The update dialog provider type.</typeparam>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="env">The host environment, used to select production or test Dialogporten endpoints.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddPatchDialogTaskForUpdates<
        TDataModel,
        TOrgNrProvider,
        TPatchOperationsProvider,
        TUpdateDialogProvider
    >(this IServiceCollection services, IHostEnvironment env)
        where TDataModel : class
        where TOrgNrProvider : class, ISubmittersOrganisasjonsnummerProvider<TDataModel>
        where TPatchOperationsProvider : class, IPatchOperationsProvider
        where TUpdateDialogProvider : class, IUpdateDialogProvider<TDataModel>
    {
        services.AddDialogportenClientForEnvironment(env);
        return services
            .AddTransient<ISubmittersOrganisasjonsnummerProvider<TDataModel>, TOrgNrProvider>()
            .AddTransient<IPatchOperationsProvider, TPatchOperationsProvider>()
            .AddTransient<IUpdateDialogProvider<TDataModel>, TUpdateDialogProvider>()
            .AddTransient<IServiceTask, UpdateDialogTask<TDataModel>>();
    }

    private static IServiceCollection AddDialogportenClientForEnvironment(
        this IServiceCollection services,
        IHostEnvironment env
    )
    {
        services.AddDialogportenClient(
            new Altinn.ApiClients.Dialogporten.DialogportenSettings
            {
                BaseUri = env.IsProduction()
                    ? "https://platform.altinn.no/dialogporten"
                    : "https://platform.tt02.altinn.no/dialogporten",
            },
            auth =>
            {
                auth.UseMaskinportenAltinnAuthorization(
                    "digdir:dialogporten.serviceprovider digdir:dialogporten.serviceprovider.search"
                );
            }
        );
        return services;
    }
}
