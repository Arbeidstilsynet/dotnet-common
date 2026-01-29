using System.Globalization;
using System.Text.Json;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

public static class AdapterExtensions
{
    /// <summary>
    /// Converts an Altinn <see cref="Instance"/> to <see cref="AltinnMetadata"/>.
    /// </summary>
    /// <param name="altinnInstance">The Altinn instance to convert.</param>
    /// <returns>The corresponding <see cref="AltinnMetadata"/> object.</returns>
    public static AltinnMetadata ToAltinnMetadata(this AltinnInstance altinnInstance)
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

    /// <summary>
    /// Converts an <see cref="AltinnInstanceSummary"/> to a metadata dictionary with an Altinn reference.
    /// </summary>
    /// <param name="altinnInstanceSummary">The Altinn instance summary to convert.</param>
    /// <returns>A dictionary containing metadata and the Altinn reference.</returns>
    public static Dictionary<string, string> ToMetadataDictionary(
        this AltinnInstanceSummary altinnInstanceSummary
    )
    {
        var dict = altinnInstanceSummary.Metadata.ToDict();

        dict["altinnReference"] =
            altinnInstanceSummary.Metadata.InstanceGuid?.ToAltinnReference() ?? "";

        return dict;
    }

    /// <summary>
    /// Converts <see cref="AltinnMetadata"/> to a metadata dictionary with an Altinn reference.
    /// </summary>
    /// <param name="altinnMetadata">The Altinn metadata to convert.</param>
    /// <returns>A dictionary containing metadata and the Altinn reference.</returns>
    public static Dictionary<string, string> ToMetadataDictionary(
        this AltinnMetadata altinnMetadata
    )
    {
        var dict = altinnMetadata.ToDict();

        dict["altinnReference"] = altinnMetadata.InstanceGuid?.ToAltinnReference() ?? "";

        return dict;
    }

    /// <summary>
    /// Converts a <see cref="Guid"/> to an Altinn reference string (last segment of the GUID).
    /// </summary>
    /// <param name="guid">The GUID to convert.</param>
    /// <returns>The Altinn reference string.</returns>
    public static string? ToAltinnReference(this Guid guid)
    {
        return guid.ToString().Split('-').LastOrDefault();
    }

    /// <summary>
    /// Converts an object's public properties to a dictionary using camelCase keys.
    /// </summary>
    /// <typeparam name="T">The type of the source object.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>A dictionary of property names and values.</returns>
    private static Dictionary<string, string> ToDict<T>(this T source)
    {
        return typeof(T)
            .GetProperties()
            .ToDictionary(
                p => JsonNamingPolicy.CamelCase.ConvertName(p.Name),
                p => p.GetValue(source) switch
                {
                    null => "",
                    IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                    var value => value.ToString() ?? ""
                }
            );
    }
}
