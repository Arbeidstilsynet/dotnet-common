using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

public static class AdapterExtensions
{
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
        };
    }

    public static InstanceRequest ToInstanceAddress(this AltinnMetadata altinnMetadata)
    {
        return new InstanceRequest()
        {
            InstanceGuid = altinnMetadata.InstanceGuid ?? Guid.Empty,
            InstanceOwnerPartyId = altinnMetadata.InstanceOwnerPartyId ?? "",
        };
    }
}
