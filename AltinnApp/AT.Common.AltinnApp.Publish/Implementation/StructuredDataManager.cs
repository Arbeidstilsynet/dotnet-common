using System.Text;
using System.Text.Json;
using Altinn.App.Core.Features;
using Altinn.App.Core.Internal.App;
using Altinn.App.Core.Internal.Data;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Microsoft.Extensions.Logging;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal class StructuredDataManager<TDataModel, TStructuredData> : IProcessTaskEnd, IProcessEnd
    where TDataModel : class
    where TStructuredData : class
{
    internal record Config
    {
        public bool IncludeErrorDetails { get; init; } = false;
        public bool DeleteAppDataModelAfterMapping { get; init; } = true;
        public Func<TDataModel, TStructuredData> MapFunc { get; init; }

        public Config(Func<TDataModel, TStructuredData> mapFunc)
        {
            MapFunc = mapFunc;
        }
    }

    private readonly IApplicationClient _applicationClient;
    private readonly IDataClient _dataClient;
    private readonly Config _config;
    private readonly ILogger<StructuredDataManager<TDataModel, TStructuredData>> _logger;

    public StructuredDataManager(
        IApplicationClient applicationClient,
        IDataClient dataClient,
        Config config,
        ILogger<StructuredDataManager<TDataModel, TStructuredData>> logger
    )
    {
        _applicationClient = applicationClient;
        _dataClient = dataClient;
        _config = config;
        _logger = logger;
    }

    public async Task End(Instance instance, List<InstanceEvent>? events)
    {
        // Right after submission

        var dataModelElement = await _applicationClient.GetRequiredDataModelElement<TDataModel>(
            instance
        );

        if (_config.DeleteAppDataModelAfterMapping)
        {
            await _dataClient.DeleteElement(instance, dataModelElement);
        }
    }

    public async Task End(string taskId, Instance instance)
    {
        // Just before submission

        try
        {
            var dataModelElement = await _applicationClient.GetRequiredDataModelElement<TDataModel>(
                instance
            );
            var dataModel = await _dataClient.GetData<TDataModel>(
                instance,
                dataModelElement,
                CancellationToken.None
            );

            var structuredData = _config.MapFunc.Invoke(dataModel);

            await _dataClient.InsertStructuredData(
                instance,
                structuredData,
                CancellationToken.None
            );
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

            if (_config.IncludeErrorDetails)
            {
                errorMessageBuilder.AppendLine().AppendLine().Append("Details: ").Append(e.Message);
            }

            throw new InvalidOperationException(errorMessageBuilder.ToString(), e);
        }
    }
}

file static class Extensions
{
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
        CancellationToken? cancellationToken = null
    )
        where T : class
    {
        var stream = structuredData.ToBinaryStream();

        await dataClient.InsertBinaryData(
            instance.Id,
            "structured-data",
            "application/json",
            "structured-data.json",
            stream,
            cancellationToken: cancellationToken ?? CancellationToken.None
        );
    }

    public static async Task DeleteElement(
        this IDataClient dataClient,
        Instance instance,
        DataElement dataElement
    )
    {
        await dataClient.DeleteData(
            instance.GetInstanceOwnerPartyId(),
            instance.GetInstanceGuid(),
            Guid.Parse(dataElement.Id),
            false
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
