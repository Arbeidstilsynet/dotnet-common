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
        // Split the Id by '/' and parse the second part as a Guid
        if (instance.Id.Split("/").Length != 2)
        {
            throw new InvalidOperationException("Instance ID must be in the format partyId/instanceGuid");
        }
        
        // Ensure the second part is a valid Guid
        if (!Guid.TryParse(instance.Id.Split("/")[1], out var instanceGuid))
        {
            throw new InvalidOperationException("Instance ID must contain a valid Guid in the second part");
        }
        
        // Return the parsed Guid
        return instanceGuid;
    }

    /// <summary>
    /// Extracts the application name from the instance's AppId
    /// </summary>
    /// <param name="instance">The instance</param>
    /// <returns>The name of the application</returns>
    public static string GetAppName(this Instance instance)
    {
        if (instance.AppId.Split("/").Length != 2)
        {
            throw new InvalidOperationException("AppId must be in the format org/app");
        }
        
        // Return the second part of the AppId, which is the application name
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
