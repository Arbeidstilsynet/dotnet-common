using Altinn.App.Core.Internal.Data;
using Altinn.Platform.Storage.Interface.Models;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

/// <summary>
/// Nyttig metoder for å interagere med en <see cref="IDataClient"/> fra en <see cref="Instance"/>.
/// </summary>
public static class DataClientExtensions
{
    /// <summary>
    /// Hent skjema-dataen av type <typeparamref name="T"/> fra en <see cref="Instance"/>.
    /// </summary>
    /// <param name="dataClient">Interface som brukes for å hente skjema-dataen</param>
    /// <param name="instance">Instansen hvor skjema-dataen ligger</param>
    /// <param name="dataType">Datatype-IDen til skjema. Default basert på konvensjon i arbeidstilsynet: "skjema"</param>
    /// <typeparam name="T">Typen til datamodellen for skjema</typeparam>
    /// <returns>Skjema-dataen av type <typeparamref name="T"/>, eller null, hvis det ikke fantes et element med typen <paramref name="dataType"/></returns>
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
    /// Slett et element fra <see cref="Instance"/>. Kan for eksempel brukes for å slette vedlegg som ikke lenger er i bruk.
    /// </summary>
    /// <param name="dataClient">Interface som brukes for å slette dataelementet</param>
    /// <param name="instance">Instansen hvor dataelementet ligger </param>
    /// <param name="element">Elementet som skal slettes</param>
    /// <param name="delay">Avgjør om slettinga kan utsettes, eller må skje nå (default)</param>
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
