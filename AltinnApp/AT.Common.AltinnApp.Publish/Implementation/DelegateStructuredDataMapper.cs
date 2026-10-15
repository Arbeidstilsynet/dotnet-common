using Arbeidstilsynet.Common.AltinnApp.Ports;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal class DelegateStructuredDataMapper<TDataModel, TStructuredData>(
    Func<TDataModel, TStructuredData> mapFunc
) : IStructuredDataMapper<TDataModel, TStructuredData>
    where TDataModel : class
    where TStructuredData : class
{
    public Task<TStructuredData> Map(TDataModel dataModel) => Task.FromResult(mapFunc(dataModel));
}
