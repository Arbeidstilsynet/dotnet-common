using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Extensions;

/// <summary>
/// Translates the package's request models into the shapes the generated storage client expects.
/// </summary>
internal static class InstanceAddressExtensions
{
    /// <summary>
    /// The generated client indexes instances by an integer party id, whereas the public request
    /// model carries it as a string.
    /// </summary>
    public static int GetInstanceOwnerPartyId(this InstanceRequest instanceRequest)
    {
        return int.TryParse(instanceRequest.InstanceOwnerPartyId, out var partyId)
            ? partyId
            : throw new ArgumentException(
                $"The instance owner party id '{instanceRequest.InstanceOwnerPartyId}' is not a valid integer.",
                nameof(instanceRequest)
            );
    }

    /// <summary>
    /// Extracts the instance address from the source URL of an Altinn cloud event, which has the
    /// form <c>.../instances/{instanceOwnerPartyId}/{instanceGuid}</c>.
    /// </summary>
    public static InstanceRequest ToInstanceRequest(this AltinnCloudEvent cloudEvent)
    {
        try
        {
            var sourcePath = cloudEvent.Source.PathAndQuery;
            var path = sourcePath[sourcePath.IndexOf("instances", StringComparison.Ordinal)..];

            var queryIndex = path.IndexOf('?');
            if (queryIndex >= 0)
            {
                path = path[..queryIndex];
            }

            var segments = path.Split('/');

            return new InstanceRequest
            {
                InstanceOwnerPartyId = segments[1],
                InstanceGuid = Guid.Parse(segments[2]),
            };
        }
        catch (Exception e)
        {
            throw new AltinnEventSourceParseException(
                $"Could not extract the instance identifier for the provided Source URL by an altinn cloud event. The source url was {cloudEvent.Source?.AbsoluteUri}",
                e
            );
        }
    }
}
