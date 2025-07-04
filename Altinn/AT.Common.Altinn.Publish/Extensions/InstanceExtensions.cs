using Altinn.Platform.Storage.Interface.Models;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

/// <summary>
/// Methods to extract commonly used information from an <see cref="Instance"/>
/// </summary>
public static class InstanceExtensions
{
    /// <summary>
    /// Extracts the Guid from the instance's Id
    /// </summary>
    /// <param name="instance">The instance</param>
    /// <returns>The ID of the instance</returns>
    public static Guid GetInstanceGuid(this Instance instance)
    {
        return Guid.Parse(instance.Id.Split("/")[1]);
    }

    /// <summary>
    /// Extracts the application name from the instance's AppId
    /// </summary>
    /// <param name="instance">The instance</param>
    /// <returns>The name of the application</returns>
    public static string GetAppName(this Instance instance)
    {
        return instance.AppId.Split("/")[1];
    }

    /// <summary>
    /// Extracts the partyId of the instance owner
    /// </summary>
    /// <param name="instance">The instance</param>
    /// <returns>InstanceOwner.PartyId</returns>
    public static int GetInstanceOwnerPartyId(this Instance instance)
    {
        return int.Parse(instance.InstanceOwner.PartyId);
    }
}
