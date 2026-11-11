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
using Microsoft.Extensions.Logging;
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
    /// Additional data type IDs to exclude from the consumer-facing attachment list. The datamodel
    /// type is automatically added to this list.
    /// </summary>
    public string[] ExcludedAttachmentDataTypeIds { get; init; } = [];

    /// <summary>
    /// Whether to include error details in the structured data in case of mapping errors. Default is false.
    /// </summary>
    public bool IncludeErrorDetails { get; init; } = false;

    /// <summary>
    /// Whether to disable validation of the structured data against the JSON schema during mapping. Default is false.
    /// </summary>
    public bool DisableValidation { get; init; } = false;

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
/// Configuration options for privacy-safe Umami analytics tracking in an Altinn application.
/// </summary>
public sealed record UmamiConfiguration
{
    /// <summary>
    /// The configuration section name in <c>appsettings.json</c> (<c>"Umami"</c>).
    /// </summary>
    public const string SectionName = "Umami";

    /// <summary>
    /// The default HTTPS origin for Altinn Studio applications in TT02 (<c>Staging</c>).
    /// </summary>
    public const string DefaultStagingAppOrigin = "https://dat.apps.tt02.altinn.no";

    /// <summary>
    /// The default HTTPS origin for Altinn Studio applications in <c>Production</c>.
    /// </summary>
    public const string DefaultProductionAppOrigin = "https://dat.apps.altinn.no";

    /// <summary>
    /// Whether to enable Umami tracking in <c>Development</c> when <c>Umami:WebsiteId</c> is configured (for example in E2E tests).
    /// Defaults to <see langword="false"/> so <c>Development</c> short-circuits by default.
    /// Can also be set via <c>Umami:EnableInDevelopment</c> in <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>.
    /// </summary>
    public bool EnableInDevelopment { get; init; } = false;

    /// <summary>
    /// Optional fallback Umami website UUID when <c>Umami:WebsiteId</c> is not set in <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>.
    /// </summary>
    public string? WebsiteId { get; init; }

    /// <summary>
    /// The origin of the Umami instance or collection gateway (for example <c>"https://umami.example.com"</c>)
    /// when <c>Umami:CollectorOrigin</c> is not set in <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>.
    /// </summary>
    public string? CollectorOrigin { get; init; }

    /// <summary>
    /// The default path of the Umami tracker script.
    /// </summary>
    public const string DefaultScriptPath = "/script.js";

    /// <summary>
    /// Optional absolute path of the tracker script on <see cref="CollectorOrigin"/> when <c>Umami:ScriptPath</c> is not set
    /// in <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>. Defaults to <see cref="DefaultScriptPath"/>.
    /// Use this when Umami runs with a custom <c>TRACKER_SCRIPT_NAME</c> or <c>BASE_PATH</c>, for example <c>"/public/tracker.js"</c>.
    /// </summary>
    public string? ScriptPath { get; init; }

    /// <summary>
    /// Optional override for the HTTPS origin where the Altinn application is hosted.
    /// Defaults to <see cref="DefaultProductionAppOrigin"/> in <c>Production</c> and <see cref="DefaultStagingAppOrigin"/> otherwise.
    /// </summary>
    public string? AppOrigin { get; init; }

    /// <summary>
    /// Optional override for the Altinn application path (for example <c>"/dat/forhandsmelding"</c>).
    /// When omitted, discovered automatically from <c>config/applicationmetadata.json</c> (<c>id</c>).
    /// </summary>
    public string? AppPath { get; init; }

    /// <summary>
    /// Optional override for the application title sent with server-side error events.
    /// When omitted, derived from the application name segment of <see cref="AppPath"/>.
    /// </summary>
    public string? AppTitle { get; init; }
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
    /// The package does not delete the form model. Enable appLogic.autoDeleteOnProcessEnd on its data type
    /// for Altinn-managed cleanup before process-completed event registration. See the README for migration details.
    /// A structured data element is always expected to exist by process end, regardless of whether the data model is kept.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="mapFunc">The function responsible for mapping from <typeparamref name="TDataModel"/> to <typeparamref name="TStructuredData"/>.</param>
    /// <param name="configuration">The structured data configuration.</param>
    /// <typeparam name="TStructuredData">The type of the structured data that the App data model will be mapped to and stored as.</typeparam>
    /// <typeparam name="TDataModel">The type of the App data model to map to structured data. Form-model cleanup is configured in application metadata.</typeparam>
    [Obsolete(
        "Implement IStructuredDataMapper<TDataModel, TStructuredData> and use AddStructuredData<TDataModel, TStructuredData, TMapper>(env, configure) instead."
    )]
    public static IServiceCollection AddStructuredData<TDataModel, TStructuredData>(
        this IServiceCollection services,
        Func<TDataModel, TStructuredData> mapFunc,
        StructuredDataConfiguration configuration
    )
        where TDataModel : class
        where TStructuredData : class
    {
        services.AddSingleton<IStructuredDataMapper<TDataModel, TStructuredData>>(
            new DelegateStructuredDataMapper<TDataModel, TStructuredData>(mapFunc)
        );

        return services.AddStructuredDataInternal<TDataModel, TStructuredData>(configuration);
    }

    /// <summary>
    /// Adds a mechanism to map the datamodel of type <typeparamref name="TDataModel"/> to structured data of type <typeparamref name="TStructuredData"/>,
    /// using the mapper <typeparamref name="TMapper"/>.
    /// <br/>
    /// The mapper is resolved from the service provider (transient), so it can depend on other services through constructor injection.
    /// <br/>
    /// The package does not delete the form model. Enable appLogic.autoDeleteOnProcessEnd on its data type
    /// for Altinn-managed cleanup before process-completed event registration. See the README for migration details.
    /// <br/>
    /// A structured data element is always expected to exist by process end, regardless of whether the data model is kept.
    /// <br/>
    /// The app instance will also declare the <see cref="DataElement.DataType"/> of the structured data and main content. This declaration is written to DataValues, based on the <see cref="StructuredDataConfiguration"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="env">The hosting environment. Used to determine configuration defaults: error details are included in non-production environments and excluded in production.</param>
    /// <param name="configure">Optional function to adjust the environment-based defaults, e.g. <c>c => c with { TaskIdFilter = ["Task_1"], ExcludedAttachmentDataTypeIds = ["internal-data"] }</c>.</param>
    /// <typeparam name="TDataModel">The type of the App data model to map to structured data. Form-model cleanup is configured in application metadata.</typeparam>
    /// <typeparam name="TStructuredData">The type of the structured data that the App data model will be mapped to and stored as.</typeparam>
    /// <typeparam name="TMapper">The <see cref="IStructuredDataMapper{TDataModel, TStructuredData}"/> implementation.</typeparam>
    /// <returns>Service collection for chaining</returns>
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
    public static IServiceCollection AddStructuredData<TDataModel, TStructuredData, TMapper>(
        this IServiceCollection services,
        IHostEnvironment env,
        Func<StructuredDataConfiguration, StructuredDataConfiguration>? configure = null
    )
        where TDataModel : class
        where TStructuredData : class
        where TMapper : class, IStructuredDataMapper<TDataModel, TStructuredData>
    {
        var configuration = new StructuredDataConfiguration
        {
            IncludeErrorDetails = !env.IsProduction(),
        };

        if (configure is not null)
        {
            configuration = configure(configuration);
        }

        services.AddTransient<IStructuredDataMapper<TDataModel, TStructuredData>, TMapper>();

        return services.AddStructuredDataInternal<TDataModel, TStructuredData>(configuration);
    }

    /// <summary>
    /// Adds a mechanism to map the datamodel of type <typeparamref name="TDataModel"/> to structured data of type <typeparamref name="TStructuredData"/>
    /// <br/>
    /// The package does not delete the form model. Enable appLogic.autoDeleteOnProcessEnd on its data type
    /// for Altinn-managed cleanup before process-completed event registration. See the README for migration details.
    /// <br/>
    /// A structured data element is always expected to exist by process end, regardless of whether the data model is kept.
    /// <br/>
    /// The app instance will also declare the <see cref="DataElement.DataType"/> of the structured data and main content. This declaration is written to DataValues, based on the <see cref="StructuredDataConfiguration"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="mapFunc">The function responsible for mapping from <typeparamref name="TDataModel"/> to <typeparamref name="TStructuredData"/>.</param>
    /// <param name="env">The hosting environment. Used to determine which defaults to use in the configuration. In production, error details will be excluded from the structured data by default, while in non-production environments, error details will be included by default.</param>
    /// <typeparam name="TStructuredData">The type of the structured data that the App data model will be mapped to and stored as.</typeparam>
    /// <typeparam name="TDataModel">The type of the App data model to map to structured data. Form-model cleanup is configured in application metadata.</typeparam>
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
    [Obsolete(
        "Implement IStructuredDataMapper<TDataModel, TStructuredData> and use AddStructuredData<TDataModel, TStructuredData, TMapper>(env, configure) instead."
    )]
    public static IServiceCollection AddStructuredData<TDataModel, TStructuredData>(
        this IServiceCollection services,
        Func<TDataModel, TStructuredData> mapFunc,
        IHostEnvironment env
    )
        where TDataModel : class
        where TStructuredData : class
    {
        services.AddSingleton<IStructuredDataMapper<TDataModel, TStructuredData>>(
            new DelegateStructuredDataMapper<TDataModel, TStructuredData>(mapFunc)
        );

        return services.AddStructuredDataInternal<TDataModel, TStructuredData>(
            new StructuredDataConfiguration { IncludeErrorDetails = !env.IsProduction() }
        );
    }

    private static IServiceCollection AddStructuredDataInternal<TDataModel, TStructuredData>(
        this IServiceCollection services,
        StructuredDataConfiguration configuration
    )
        where TDataModel : class
        where TStructuredData : class
    {
        ValidateSignificantDataTypes(configuration);

        if (
            configuration.ExcludedAttachmentDataTypeIds is null
            || configuration.ExcludedAttachmentDataTypeIds.Any(string.IsNullOrWhiteSpace)
        )
        {
            throw new ArgumentException(
                "ExcludedAttachmentDataTypeIds cannot contain null, empty, or whitespace entries.",
                nameof(configuration)
            );
        }

        if (
            configuration.ExcludedAttachmentDataTypeIds.Any(id =>
                string.Equals(
                    id,
                    configuration.MainPdfDataTypeId,
                    StringComparison.OrdinalIgnoreCase
                )
                || string.Equals(
                    id,
                    configuration.StructuredDataTypeId,
                    StringComparison.OrdinalIgnoreCase
                )
            )
        )
        {
            throw new ArgumentException(
                "ExcludedAttachmentDataTypeIds cannot contain MainPdfDataTypeId or StructuredDataTypeId.",
                nameof(configuration)
            );
        }

        services.AddSingleton(
            new StructuredDataManager<TDataModel, TStructuredData>.Config
            {
                StructuredDataConfiguration = configuration,
            }
        );
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

    private static void ValidateSignificantDataTypes(StructuredDataConfiguration configuration)
    {
        if (
            string.IsNullOrWhiteSpace(configuration.MainPdfDataTypeId)
            || string.IsNullOrWhiteSpace(configuration.StructuredDataTypeId)
        )
        {
            throw new ArgumentException(
                "MainPdfDataTypeId and StructuredDataTypeId cannot be null, empty, or whitespace.",
                nameof(configuration)
            );
        }

        if (
            string.Equals(
                configuration.MainPdfDataTypeId,
                configuration.StructuredDataTypeId,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            throw new ArgumentException(
                "MainPdfDataTypeId and StructuredDataTypeId must be different.",
                nameof(configuration)
            );
        }
    }

    /// <summary>
    /// Registers a <see cref="IStructuredDataPostProcessor"/> that runs at process end, from inside the structured data feature
    /// (see <see cref="AddStructuredData{TDataModel, TStructuredData, TMapper}(IServiceCollection, IHostEnvironment, Func{StructuredDataConfiguration, StructuredDataConfiguration})"/>).
    /// <br/>
    /// Post-processors run after Altinn-managed automatic form-model deletion, and are guaranteed that the
    /// structured data element and the main PDF element exist on the instance. They must not depend on the form model.
    /// Multiple post-processors run in registration order,
    /// and should be idempotent since process end may be retried.
    /// </summary>
    /// <typeparam name="T">The post-processor implementation.</typeparam>
    /// <param name="services"></param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddStructuredDataPostProcessor<T>(
        this IServiceCollection services
    )
        where T : class, IStructuredDataPostProcessor
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient<IStructuredDataPostProcessor, T>());
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

    /// <summary>
    /// Registers <see cref="IUmamiConnector"/> for Umami analytics in an Altinn application, and reports exceptions
    /// logged at <see cref="LogLevel.Error"/> or higher to Umami as <c>altinn-error</c> events.
    /// <br/>
    /// To complete the integration, you must also update <c>App/views/Home/Index.cshtml</c> to inject
    /// <c>@inject Arbeidstilsynet.Common.AltinnApp.Ports.IUmamiConnector UmamiConnector</c> and render
    /// <c>@UmamiConnector.RenderScriptTag()</c> inside <c>&lt;head&gt;</c>.
    /// </summary>
    /// <remarks>
    /// Tracking short-circuits by default in <c>Development</c> (unless <see cref="UmamiConfiguration.EnableInDevelopment"/> or
    /// <c>Umami:EnableInDevelopment=true</c> is set) or when <c>Umami:WebsiteId</c> is empty.
    /// </remarks>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="configure">Optional callback to configure fallback options when not specified in <c>appsettings</c>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddUmamiTracking(
        this IServiceCollection services,
        Func<UmamiConfiguration, UmamiConfiguration>? configure = null
    )
    {
        var config = configure is not null
            ? configure(new UmamiConfiguration())
            : new UmamiConfiguration();
        services.AddSingleton(config);
        services.TryAddSingleton<IUmamiConnector, UmamiConnector>();
        services.TryAddSingleton<UmamiErrorQueue>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, UmamiErrorLoggerProvider>()
        );
        services.AddHostedService<UmamiErrorReporter>();
        services.AddHttpClient(
            UmamiErrorReporter.HttpClientName,
            client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(UmamiErrorReporter.UserAgent);
            }
        );
        return services;
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
