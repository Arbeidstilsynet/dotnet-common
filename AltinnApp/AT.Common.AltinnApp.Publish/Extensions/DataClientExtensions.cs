using Altinn.App.Core.Internal.Data;
using Altinn.Platform.Storage.Interface.Models;

namespace Arbeidstilsynet.Common.AltinnApp.Extensions;

/// <summary>
/// Useful methods for interacting with an <see cref="IDataClient"/> from an <see cref="Instance"/>.
/// </summary>
public static class DataClientExtensions
{
    /// <summary>
    /// Get form data of type <typeparamref name="T"/> from an <see cref="Instance"/>.
    /// </summary>
    /// <param name="dataClient">Interface used to retrieve the form data</param>
    /// <param name="instance">The instance where the form data is located</param>
    /// <param name="dataType">The datatype ID for the form. Default based on Arbeidstilsynet convention: "skjema"</param>
    /// <typeparam name="T">The type of the data model for the form</typeparam>
    /// <returns>The form data of type <typeparamref name="T"/>, or null if no element with the type <paramref name="dataType"/> was found</returns>
    public static async Task<T?> GetSkjemaData<T>(
        this IDataClient dataClient,
        Instance instance,
        string dataType = "skjema"
    )
    {
        var element = instance.Data.FirstOrDefault(d => d.DataType.Equals(dataType));

        if (element == null)
        {
            return default;
        }

        return (T?)
            await dataClient.GetFormData(
                instance.GetInstanceGuid(),
                typeof(T),
                instance.Org,
                instance.GetAppName(),
                instance.GetInstanceOwnerPartyId(),
                Guid.Parse(element.Id)
            );
    }

    /// <summary>
    /// Delete an element from an <see cref="Instance"/>. Can for example be used to delete attachments that are no longer in use.
    /// </summary>
    /// <param name="dataClient">Interface used to delete the data element</param>
    /// <param name="instance">The instance where the data element is located</param>
    /// <param name="element">The element to be deleted</param>
    /// <param name="delay">Determines whether the deletion can be postponed, or must happen now (default)</param>
    public static async Task DeleteElement(
        this IDataClient dataClient,
        Instance instance,
        DataElement element,
        bool delay = false
    )
    {
        await dataClient.DeleteData(
            instance.Org,
            instance.GetAppName(),
            instance.GetInstanceOwnerPartyId(),
            instance.GetInstanceGuid(),
            Guid.Parse(element.Id),
            delay
        );

        instance.Data.RemoveAll(e => e.Id == element.Id);
    }
}
