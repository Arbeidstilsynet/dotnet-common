
namespace Arbeidstilsynet.Common.Altinn.Abstract.Processing;

/// <summary>
/// Base class for processing members of a data model.
/// </summary>
/// <typeparam name="TDataModel"></typeparam>
/// <typeparam name="TMember"></typeparam>
public abstract class MemberProcessor<TDataModel, TMember> : BaseDataProcessor<TDataModel>
    where TDataModel : class
{
    /// <summary>
    /// Access the member of the data model that should be processed.
    /// </summary>
    /// <param name="dataModel"></param>
    /// <returns></returns>
    protected abstract TMember? AccessMember(TDataModel dataModel);
    
    /// <inheritdoc />
    protected sealed override Task ProcessData(TDataModel currentDataModel, TDataModel? previousDataModel)
    {
        if (previousDataModel is null)
            return Task.CompletedTask;
        
        var currentMember = AccessMember(currentDataModel);
        var previousMember = AccessMember(previousDataModel);
        
        if (
            currentMember is null && previousMember is null
            || (currentMember is not null && previousMember is not null && currentMember.Equals(previousMember)))
            return Task.CompletedTask;
        
        return ProcessMember(currentMember, previousMember, currentDataModel, previousDataModel);
    }
    
    /// <summary>
    /// Process the member after a change. It is up to the implementation to decide if the member should be updated or not.
    /// This is only called <see cref="previousMember"/>.Equals(<see cref="currentMember"/>) is false, or if _one_ of them is null.
    /// </summary>
    /// <param name="currentMember"></param>
    /// <param name="previousMember"></param>
    /// <param name="currentDataModel"></param>
    /// <param name="previousDataModel"></param>
    /// <returns></returns>
    protected abstract Task ProcessMember(TMember? currentMember, TMember? previousMember, TDataModel currentDataModel, TDataModel previousDataModel);
}