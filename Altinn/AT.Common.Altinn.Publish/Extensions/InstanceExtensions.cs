using Altinn.Platform.Storage.Interface.Models;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

/// <summary>
/// Metoder for å hente ut ofte brukt informasjon fra en <see cref="Instance"/>
/// </summary>
public static class InstanceExtensions
{
    /// <summary>
    /// Henter ut Guid fra instansens Id
    /// </summary>
    /// <param name="instance">Instansen</param>
    /// <returns>IDen til instansen</returns>
    public static Guid GetInstanceGuid(this Instance instance)
    {
        return Guid.Parse(instance.Id.Split("/")[1]);
    }

    /// <summary>
    /// Henter ut applikasjonsnavn fra instansens AppId
    /// </summary>
    /// <param name="instance">Instansen</param>
    /// <returns>Navnet til applikasjonen</returns>
    public static string GetAppName(this Instance instance)
    {
        return instance.AppId.Split("/")[1];
    }

    /// <summary>
    /// Henter ut partyId til eieren av instansen
    /// </summary>
    /// <param name="instance">Instansen</param>
    /// <returns>InstanceOwner.PartyId</returns>
    public static int GetInstanceOwnerPartyId(this Instance instance)
    {
        return int.Parse(instance.InstanceOwner.PartyId);
    }
}
