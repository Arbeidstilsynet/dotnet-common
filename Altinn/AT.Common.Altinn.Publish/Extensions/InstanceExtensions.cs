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
    /// <param name="AltinnInstance"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Guid GetInstanceGuid(this AltinnInstance AltinnInstance)
    {
        // Split the Id by '/' and parse the second part as a Guid
        if (AltinnInstance.Id.Split("/").Length != 2)
        {
            throw new InvalidOperationException(
                "AltinnInstance ID must be in the format partyId/instanceGuid"
            );
        }

        // Ensure the second part is a valid Guid
        if (!Guid.TryParse(AltinnInstance.Id.Split("/")[1], out var instanceGuid))
        {
            throw new InvalidOperationException(
                "AltinnInstance ID must contain a valid Guid in the second part"
            );
        }

        // Return the parsed Guid
        return instanceGuid;
    }
}
