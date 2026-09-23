using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

/// <summary>
/// Methods to extract commonly used information from an <see cref="AltinnInstance"/>
/// </summary>
public static class InstanceExtensions
{
    /// <summary>
    /// Extracts the AltinnInstance Guid from the AltinnInstance Id
    /// </summary>
    /// <param name="altinnInstance">The instance whose identifier to parse.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Guid GetInstanceGuid(this AltinnInstance altinnInstance)
    {
        if (altinnInstance.Id?.Split('/') is not [_, var instanceId])
        {
            throw new InvalidOperationException(
                "AltinnInstance ID must be in the format partyId/instanceGuid"
            );
        }

        if (!Guid.TryParse(instanceId, out var instanceGuid))
        {
            throw new InvalidOperationException(
                "AltinnInstance ID must contain a valid Guid in the second part"
            );
        }

        return instanceGuid;
    }
}
