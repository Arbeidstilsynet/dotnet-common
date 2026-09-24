using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;
using GeneratedInstanceQueryParameters = Arbeidstilsynet.Common.Altinn.Storage.Instances.InstancesRequestBuilder.InstancesRequestBuilderGetQueryParameters;

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
    public static Guid ToInstanceGuid(this AltinnCloudEvent cloudEvent) =>
        cloudEvent.ToInstanceRequest().InstanceGuid;

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

    /// <summary>
    /// Maps the package's query parameters onto the generated ones.
    /// </summary>
    /// <remarks>
    /// The mapping is explicit rather than reflective because the generated property names differ
    /// from the wire names for every dotted parameter (<c>process.isComplete</c> becomes
    /// <c>ProcessIsComplete</c>), and because the date-range parameters are expressed as arrays of
    /// comparison expressions rather than plain values.
    /// </remarks>
    public static void ApplyTo(
        this InstanceQueryParameters source,
        GeneratedInstanceQueryParameters target
    )
    {
        target.Org = source.Org;
        target.AppId = source.AppId;
        target.ProcessCurrentTask = source.ProcessCurrentTask;
        target.ProcessIsComplete = source.ProcessIsComplete;
        target.ProcessEndEvent = source.ProcessEndEvent;
        target.ProcessEnded = source.ProcessEnded.ToQueryValues();
        target.InstanceOwnerPartyId = source.InstanceOwnerPartyId;
        target.LastChanged = source.LastChanged.ToQueryValues();
        target.Created = source.Created.ToQueryValues();
        target.VisibleAfter = source.VisibleAfter.ToQueryValues();
        target.DueBefore = source.DueBefore.ToQueryValues();
        target.ExcludeConfirmedBy = source.ExcludeConfirmedBy;
        target.Confirmed = source.Confirmed;
        target.StatusIsSoftDeleted = source.IsSoftDeleted;
        target.StatusIsHardDeleted = source.IsHardDeleted;
        target.StatusIsArchived = source.IsArchived;
        target.ContinuationToken = source.ContinuationToken;
        target.Size = source.Size;
        target.MainVersionInclude = source.MainVersionInclude;
        target.MainVersionExclude = source.MainVersionExclude;
        target.SearchString = source.SearchString;
        target.Order = source.SortBy;
    }

    private static string[]? ToQueryValues(this AltinnDateTimeQuery[]? queries)
    {
        if (queries is null || queries.Length == 0)
        {
            return null;
        }

        return [.. queries.Select(query => query.ToString())];
    }
}
