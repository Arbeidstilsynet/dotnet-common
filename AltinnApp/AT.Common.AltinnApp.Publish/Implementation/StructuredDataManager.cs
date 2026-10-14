using System.Text;
using System.Text.Json;
using Altinn.App.Core.Features;
using Altinn.App.Core.Internal.App;
using Altinn.App.Core.Internal.Data;
using Altinn.App.Core.Internal.Instances;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.DependencyInjection;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Arbeidstilsynet.Common.AltinnApp.Model;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.Extensions.Logging;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal class StructuredDataManager<TDataModel, TStructuredData> : IProcessTaskEnd, IProcessEnd
    where TDataModel : class
    where TStructuredData : class
{
    internal const string StructuredDataTypeIdKey = "StructuredDataTypeId";
    internal const string MainPdfDataTypeId = "MainPdfDataTypeId";
    internal const string ExcludedAttachmentDataTypeIdsKey = "ExcludedAttachmentDataTypeIds";

    internal record Config
    {
        public StructuredDataConfiguration StructuredDataConfiguration { get; init; } =
            new StructuredDataConfiguration();
    }

    private readonly IApplicationClient _applicationClient;
    private readonly IDataClient _dataClient;
    private readonly IInstanceClient _instanceClient;
    private readonly Config _config;
    private readonly ILogger<StructuredDataManager<TDataModel, TStructuredData>> _logger;
    private readonly IStructuredDataValidator<TStructuredData> _structuredDataValidator;
    private readonly IStructuredDataMapper<TDataModel, TStructuredData> _mapper;
    private readonly IReadOnlyList<IStructuredDataPostProcessor> _postProcessors;

    public StructuredDataManager(
        IApplicationClient applicationClient,
        IDataClient dataClient,
        IInstanceClient instanceClient,
        Config config,
        ILogger<StructuredDataManager<TDataModel, TStructuredData>> logger,
        IStructuredDataValidator<TStructuredData> structuredDataValidator,
        IStructuredDataMapper<TDataModel, TStructuredData> mapper,
        IEnumerable<IStructuredDataPostProcessor> postProcessors
    )
    {
        _applicationClient = applicationClient;
        _dataClient = dataClient;
        _instanceClient = instanceClient;
        _config = config;
        _logger = logger;
        _structuredDataValidator = structuredDataValidator;
        _mapper = mapper;
        _postProcessors = postProcessors.ToList();
    }

    public async Task End(Instance instance, List<InstanceEvent>? events)
    {
        // Right after submission

        var dataModelElement = await _applicationClient.GetRequiredDataModelElement<TDataModel>(
            instance
        );

        var structuredDataExists = instance.Data.Any(d =>
            string.Equals(
                d.DataType,
                _config.StructuredDataConfiguration.StructuredDataTypeId,
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (!structuredDataExists)
        {
            throw new InvalidOperationException(
                $"No structured data element of type '{_config.StructuredDataConfiguration.StructuredDataTypeId}' was found on the instance by process end. "
                    + "This may indicate a misconfiguration or that structured data mapping did not run for this task."
            );
        }

        // Run before the destructive deletion so a failing post-processor can be retried.
        await RunPostProcessors(instance);

        if (!_config.StructuredDataConfiguration.KeepAppDataModelAfterMapping)
        {
            await _dataClient.DeleteElement(instance, dataModelElement);
        }
    }

    private async Task RunPostProcessors(Instance instance)
    {
        if (_postProcessors.Count == 0)
        {
            return;
        }

        // The instance passed to IProcessEnd is not updated with data elements created during the
        // request (e.g. the PDF generated at task end), so fetch a fresh copy from storage.
        var freshInstance = await _instanceClient.GetInstance(instance);

        var context = new StructuredDataPostProcessingContext
        {
            Instance = freshInstance,
            StructuredDataElement = freshInstance.GetRequiredLatestDataElement(
                _config.StructuredDataConfiguration.StructuredDataTypeId
            ),
            MainPdfElement = freshInstance.GetRequiredLatestDataElement(
                _config.StructuredDataConfiguration.MainPdfDataTypeId
            ),
        };

        foreach (var postProcessor in _postProcessors)
        {
            await postProcessor.Process(context);
        }
    }

    public async Task End(string taskId, Instance instance)
    {
        // Just before submission

        var taskIdFilter = _config.StructuredDataConfiguration.TaskIdFilter ?? [];
        if (
            taskIdFilter.Length > 0
            && !taskIdFilter.Contains(taskId, StringComparer.OrdinalIgnoreCase)
        )
        {
            var filter = string.Join(',', taskIdFilter);
            _logger.LogInformation(
                "Skipping structured data generation for instance {InstanceId} on task {TaskId} due to not matching any of the configured TaskIds({Filter})",
                instance.Id,
                taskId,
                filter
            );
            return;
        }

        try
        {
            var dataModelDataTypeId = await CreateStructuredData(instance);
            await CreateAppSpecification(instance, dataModelDataTypeId);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error while generating structured data for instance {InstanceId}",
                instance.Id
            );

            var errorMessageBuilder = new StringBuilder(
                "An unexpected error occurred while generating structured data. Please try again later."
            );

            if (_config.StructuredDataConfiguration.IncludeErrorDetails)
            {
                errorMessageBuilder.AppendLine().AppendLine().Append("Details: ").Append(e.Message);
            }

            throw new InvalidOperationException(errorMessageBuilder.ToString(), e);
        }
    }

    private async Task<string> CreateStructuredData(Instance instance)
    {
        var dataModelElement = await _applicationClient.GetRequiredDataModelElement<TDataModel>(
            instance
        );
        var dataModel = await _dataClient.GetData<TDataModel>(
            instance,
            dataModelElement,
            CancellationToken.None
        );

        var structuredData = await _mapper.Map(dataModel);

        if (!_config.StructuredDataConfiguration.DisableValidation)
        {
            await _structuredDataValidator.ValidateAndThrow(structuredData);
        }

        await DeleteExistingStructuredData(instance);

        await _dataClient.InsertStructuredData(
            instance,
            structuredData,
            _config.StructuredDataConfiguration.StructuredDataTypeId,
            CancellationToken.None
        );

        return dataModelElement.DataType;
    }

    private async Task DeleteExistingStructuredData(Instance instance)
    {
        var existingStructuredDataElements = instance
            .Data.Where(d =>
                string.Equals(
                    d.DataType,
                    _config.StructuredDataConfiguration.StructuredDataTypeId,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .ToList();

        foreach (var element in existingStructuredDataElements)
        {
            await _dataClient.DeleteElement(instance, element);
        }
    }

    private async Task CreateAppSpecification(Instance instance, string dataModelDataTypeId)
    {
        var configuration = _config.StructuredDataConfiguration;
        if (
            string.Equals(
                dataModelDataTypeId,
                configuration.MainPdfDataTypeId,
                StringComparison.OrdinalIgnoreCase
            )
            || string.Equals(
                dataModelDataTypeId,
                configuration.StructuredDataTypeId,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            throw new InvalidOperationException(
                $"The datamodel data type '{dataModelDataTypeId}' cannot be the same as MainPdfDataTypeId or StructuredDataTypeId."
            );
        }

        var excludedAttachmentDataTypeIds = configuration
            .ExcludedAttachmentDataTypeIds.Append(dataModelDataTypeId)
            .Distinct(StringComparer.Ordinal);

        var dataValues = new Dictionary<string, string?>()
        {
            { StructuredDataTypeIdKey, configuration.StructuredDataTypeId },
            { MainPdfDataTypeId, configuration.MainPdfDataTypeId },
            {
                ExcludedAttachmentDataTypeIdsKey,
                JsonSerializer.Serialize(excludedAttachmentDataTypeIds)
            },
        };

        await _instanceClient.UpdateDataValues(instance, dataValues);
    }
}

file static class Extensions
{
    public static DataElement GetRequiredLatestDataElement(this Instance instance, string dataType)
    {
        return instance
                .Data.Where(d =>
                    string.Equals(d.DataType, dataType, StringComparison.OrdinalIgnoreCase)
                )
                .OrderByDescending(d => d.Created)
                .FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"No data element of type '{dataType}' was found on instance {instance.Id} before structured data post-processing."
            );
    }

    public static async Task<DataElement> GetRequiredDataModelElement<T>(
        this IApplicationClient applicationClient,
        Instance instance
    )
    {
        var type = typeof(T);

        var application = await applicationClient.GetApplication(
            instance.Org,
            instance.GetAppName()
        );

        if (application == null)
        {
            throw new InvalidOperationException("Could not retrieve application metadata");
        }

        var dataType = application.DataTypes.FirstOrDefault(d =>
            d.AppLogic?.ClassRef == type.FullName
        );

        if (dataType == null)
        {
            throw new InvalidOperationException(
                $"Could not find data type for {type.FullName} in application metadata"
            );
        }

        var dataModelElement = instance.Data.FirstOrDefault(d => d.DataType == dataType.Id);

        if (dataModelElement == null)
        {
            throw new InvalidOperationException(
                $"Could not find data element for data type {dataType.Id} in instance data"
            );
        }

        return dataModelElement;
    }

    public static async Task<T> GetData<T>(
        this IDataClient dataClient,
        Instance instance,
        DataElement dataElement,
        CancellationToken? cancellationToken = null
    )
        where T : class
    {
        if (
            await dataClient.GetFormData(
                instance,
                dataElement,
                cancellationToken: cancellationToken ?? CancellationToken.None
            )
            is not T data
        )
        {
            throw new InvalidOperationException(
                $"Could not retrieve data model of type {typeof(T).FullName} from data element {dataElement.Id}"
            );
        }

        return data;
    }

    public static async Task InsertStructuredData<T>(
        this IDataClient dataClient,
        Instance instance,
        T structuredData,
        string structuredDataTypeId,
        CancellationToken? cancellationToken = null
    )
        where T : class
    {
        using var stream = structuredData.ToBinaryStream();

        await dataClient.InsertBinaryData(
            instance.Id,
            structuredDataTypeId,
            "application/json",
            "structured-data.json",
            stream,
            cancellationToken: cancellationToken ?? CancellationToken.None
        );
    }

    public static async Task DeleteElement(
        this IDataClient dataClient,
        Instance instance,
        DataElement dataElement,
        CancellationToken? cancellationToken = null
    )
    {
        await dataClient.DeleteData(
            instance.GetInstanceOwnerPartyId(),
            instance.GetInstanceGuid(),
            Guid.Parse(dataElement.Id),
            false,
            cancellationToken: cancellationToken ?? CancellationToken.None
        );
        instance.Data.Remove(dataElement);
    }

    public static Stream ToBinaryStream<T>(this T structuredData)
        where T : class
    {
        var json = JsonSerializer.Serialize(structuredData);

        // Put the string into a memory stream
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var stream = new MemoryStream(bytes);
        return stream;
    }
}
