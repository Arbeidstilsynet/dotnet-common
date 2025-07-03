using Altinn.App.Core.Features;
using Altinn.Platform.Storage.Interface.Models;

namespace Arbeidstilsynet.Common.Altinn.Abstract.Processing;

/// <summary>
/// Base class for data processors that handle specific data types.
/// ProcessDataWrite is only called when the data is of type T.
/// </summary>
/// <typeparam name="TDataModel"></typeparam>
public abstract class BaseDataProcessor<TDataModel> : IDataProcessor
    where TDataModel : class
{
    /// <inheritdoc />
    public Task ProcessDataRead(
        Instance instance,
        Guid? dataId,
        object data,
        string? language
    ) => Task.CompletedTask;

    /// <inheritdoc />
    public Task ProcessDataWrite(
        Instance instance,
        Guid? dataId,
        object data,
        object? previousData,
        string? language
    )
    {
        if (data is not TDataModel current)
        {
            return Task.CompletedTask;
        }

        return ProcessData(current, previousData as TDataModel);
    }

    /// <summary>
    /// Processes the data model. If previous is null, it indicates that this is the first time the data is being processed.
    /// </summary>
    /// <param name="currentDataModel"></param>
    /// <param name="previousDataModel"></param>
    /// <returns></returns>
    protected abstract Task ProcessData(TDataModel currentDataModel, TDataModel? previousDataModel);
}
