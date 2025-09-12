using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

/// <summary>
/// Methods to extract commonly used information from an <see cref="Instance"/>
/// </summary>
public static class InstanceExtensions
{
    public static Guid GetInstanceGuid(this AltinnInstance instance)
    {
        // Split the Id by '/' and parse the second part as a Guid
        if (instance.Id.Split("/").Length != 2)
        {
            throw new InvalidOperationException(
                "Instance ID must be in the format partyId/instanceGuid"
            );
        }

        // Ensure the second part is a valid Guid
        if (!Guid.TryParse(instance.Id.Split("/")[1], out var instanceGuid))
        {
            throw new InvalidOperationException(
                "Instance ID must contain a valid Guid in the second part"
            );
        }

        // Return the parsed Guid
        return instanceGuid;
    }
}
