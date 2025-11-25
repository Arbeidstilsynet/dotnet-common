using Altinn.App.Core.Features;
using Altinn.App.Core.Internal.App;
using Altinn.App.Core.Internal.Data;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Extensions;

namespace Arbeidstilsynet.Common.AltinnApp.Abstract.Processing;

/// <summary>
/// Abstract processor for processing data model before submission
/// </summary>
/// <typeparam name="TDataModel"></typeparam>
/// <remarks><typeparamref name="TDataModel"/> must be the data model for the application (i.e. must have a ClassRef)</remarks>
public abstract class PreSubmitDataModelProcessor<TDataModel> : IProcessTaskEnd
    where TDataModel : class
{
    private readonly IDataClient _dataClient;
    private readonly IApplicationClient _applicationClient;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dataClient"></param>
    /// <param name="applicationClient"></param>
    protected PreSubmitDataModelProcessor(
        IDataClient dataClient,
        IApplicationClient applicationClient
    )
    {
        _dataClient = dataClient;
        _applicationClient = applicationClient;
    }

    /// <summary>
    /// Process the data model before submission. This is called by the Altinn App runtime.
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="instance"></param>
    public async Task End(string taskId, Instance instance)
    {
        var dataElement = await _applicationClient.GetRequiredDataModelElement<TDataModel>(
            instance
        );

        var data = await _dataClient.GetData<TDataModel>(instance, dataElement);

        var processedDataModel = await ProcessDataModel(data, instance);

        await _dataClient.UpdateDataElement(instance, dataElement, processedDataModel);
    }

    /// <summary>
    /// Process the data model before submission. The result will be saved as the new data model.
    /// </summary>
    /// <param name="currentDataModel"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    protected abstract Task<TDataModel> ProcessDataModel(
        TDataModel currentDataModel,
        Instance instance
    );
}
