using Altinn.App.Core.Internal.App;
using Altinn.Platform.Storage.Interface.Models;

namespace Arbeidstilsynet.Common.AltinnApp.Extensions;

/// <summary>
/// Extension methods for IApplicationClient
/// </summary>
public static class ApplicationClientExtensions
{
    /// <summary>
    /// Retrieves the data model element of type <typeparamref name="T"/> from the given instance.
    /// </summary>
    /// <param name="applicationClient"></param>
    /// <param name="instance"></param>
    /// <typeparam name="T">This must be the data model of the application (i.e. have AppLogic.ClassRef set to the full name of this type)</typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If the application or datamodel can't be found</exception>
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
}
