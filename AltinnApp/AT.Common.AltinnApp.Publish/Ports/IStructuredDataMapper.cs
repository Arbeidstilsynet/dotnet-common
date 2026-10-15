namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Maps the App data model of type <typeparamref name="TDataModel"/> to structured data of type <typeparamref name="TStructuredData"/>.
/// </summary>
/// <remarks>
/// Implementations are resolved from the service provider, so they can use constructor injection
/// (e.g. <see cref="ILandskodeLookup"/>). Register with
/// <c>services.AddStructuredData&lt;TDataModel, TStructuredData, TMapper&gt;(env)</c>.
/// </remarks>
/// <typeparam name="TDataModel">The type of the App data model.</typeparam>
/// <typeparam name="TStructuredData">The type of the structured data that will be stored.</typeparam>
public interface IStructuredDataMapper<in TDataModel, TStructuredData>
    where TDataModel : class
    where TStructuredData : class
{
    /// <summary>
    /// Maps the given data model to structured data.
    /// </summary>
    /// <param name="dataModel">The App data model of the instance.</param>
    /// <returns>The structured data to store on the instance.</returns>
    Task<TStructuredData> Map(TDataModel dataModel);
}
