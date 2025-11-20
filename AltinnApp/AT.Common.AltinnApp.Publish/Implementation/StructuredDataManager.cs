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

        await _dataClient.DeleteElement(instance, dataModelElement);
    }

    public async Task End(string taskId, Instance instance)
    {
        // Just before submission

        try
        {
            var dataModelElement = await _applicationClient.GetRequiredDataModelElement<TDataModel>(
                instance
            );
            var dataModel = await _dataClient.GetData<TDataModel>(instance, dataModelElement);

            var structuredData = _config.MapFunc.Invoke(dataModel);

            await _dataClient.InsertStructuredData(instance, structuredData);
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
    public static async Task InsertStructuredData<T>(
        this IDataClient dataClient,
        Instance instance,
        T structuredData
    )
        where T : class
    {
        var stream = structuredData.ToBinaryStream();

        await dataClient.InsertBinaryData(
            instance.Id,
            "structured-data",
            "application/json",
            "structured-data.json",
            stream
        );
    }

    private static Stream ToBinaryStream<T>(this T structuredData)
        where T : class
    {
        var json = JsonSerializer.Serialize(structuredData);

        // Put the string into a memory stream
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var stream = new MemoryStream(bytes);
        return stream;
    }
}
