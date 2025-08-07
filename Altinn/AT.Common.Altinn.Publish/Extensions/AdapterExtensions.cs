using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

public static class AdapterExtensions
{
    /// <summary>
    /// Converts an Altinn <see cref="Instance"/> to <see cref="AltinnMetadata"/>.
    /// </summary>
    /// <param name="altinnInstance">The Altinn instance to convert.</param>
    /// <returns>The corresponding <see cref="AltinnMetadata"/> object.</returns>
    public static AltinnMetadata ToAltinnMetadata(this Instance altinnInstance)
    {
        var appIdParts = altinnInstance.AppId?.Split('/');

        if (appIdParts is not { Length: 2 })
        {
            throw new InvalidOperationException("AppId must be in the format org/app");
        }

        var org = appIdParts[0];
        var app = appIdParts[1];

        var instanceIdParts = altinnInstance.Id?.Split('/');

        if (instanceIdParts is not { Length: 2 })
        {
            throw new InvalidOperationException(
                "InstanceId must be in the format partyId/instanceGuid"
            );
        }

        var partyId = instanceIdParts[0];
        var instanceGuid = Guid.Parse(instanceIdParts[1]);

        return new AltinnMetadata()
        {
            App = app,
            Org = org,
            InstanceGuid = instanceGuid,
            InstanceOwnerPartyId = partyId,
            OrganisationNumber = altinnInstance.InstanceOwner.OrganisationNumber,
            ProcessStarted = altinnInstance.Process.Started,
            ProcessEnded = altinnInstance.Process.Ended,
        };
    }

    /// <summary>
    /// Converts <see cref="AltinnMetadata"/> to an <see cref="InstanceRequest"/>.
    /// </summary>
    /// <param name="altinnMetadata">The Altinn metadata to convert.</param>
    /// <returns>The corresponding <see cref="InstanceRequest"/>.</returns>
    public static InstanceRequest ToInstanceAddress(this AltinnMetadata altinnMetadata)
    {
        return new InstanceRequest()
        {
            InstanceGuid = altinnMetadata.InstanceGuid ?? Guid.Empty,
            InstanceOwnerPartyId = altinnMetadata.InstanceOwnerPartyId ?? "",
        };
    }
}
