namespace Arbeidstilsynet.Common.Altinn.Abstract.Processing;

/// <summary>
/// Processes changes in a list of items within a data model.
/// </summary>
/// <typeparam name="TDataModel"></typeparam>
/// <typeparam name="TListItem"></typeparam>
public abstract class ListProcessor<TDataModel, TListItem>
    : MemberProcessor<TDataModel, List<TListItem>>
    where TDataModel : class
{
    /// <inheritdoc />
    protected sealed override async Task ProcessMember(
        List<TListItem>? currentList,
        List<TListItem>? previousList,
        TDataModel currentDataModel,
        TDataModel previousDataModel
    )
    {
        if (currentList?.Count != previousList?.Count)
        {
            await ProcessListChange(currentList, previousList, currentDataModel, previousDataModel);
        }
        else if (currentList is not null && previousList is not null)
        {
            foreach (var (currentItem, previousItem) in currentList.Zip(previousList))
            {
                if (currentItem is null && previousItem is null)
                {
                    continue; // Both items are null, nothing to process
                }

                if (currentItem is null || previousItem is null)
                {
                    // One of the items is null, treat it as a change
                    await ProcessItem(
                        currentItem,
                        previousItem,
                        currentDataModel,
                        previousDataModel
                    );
                }
                else if (!currentItem.Equals(previousItem))
                {
                    // If the items are not equal, process the change
                    await ProcessItem(
                        currentItem,
                        previousItem,
                        currentDataModel,
                        previousDataModel
                    );
                }
            }
        }
    }

    /// <summary>
    /// Processes the change in the list when the count of items has changed.
    /// </summary>
    /// <param name="currentList"></param>
    /// <param name="previousList"></param>
    /// <param name="currentDataModel"></param>
    /// <param name="previousDataModel"></param>
    /// <returns></returns>
    protected virtual Task ProcessListChange(
        List<TListItem>? currentList,
        List<TListItem>? previousList,
        TDataModel currentDataModel,
        TDataModel previousDataModel
    ) => Task.CompletedTask;

    /// <summary>
    ///
    /// </summary>
    /// <param name="currentItem"></param>
    /// <param name="previousItem"></param>
    /// <param name="currentDataModel"></param>
    /// <param name="previousDataModel"></param>
    /// <returns></returns>
    protected virtual Task ProcessItem(
        TListItem currentItem,
        TListItem previousItem,
        TDataModel currentDataModel,
        TDataModel previousDataModel
    ) => Task.CompletedTask;
}
