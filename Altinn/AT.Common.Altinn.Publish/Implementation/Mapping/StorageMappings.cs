using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using GeneratedDataElement = Arbeidstilsynet.Common.Altinn.Storage.Models.DataElement;
using GeneratedFileScanResult = Arbeidstilsynet.Common.Altinn.Storage.Models.FileScanResult;
using GeneratedInstance = Arbeidstilsynet.Common.Altinn.Storage.Models.Instance;
using GeneratedInstanceQueryResponse = Arbeidstilsynet.Common.Altinn.Storage.Models.InstanceQueryResponse;
using GeneratedKeyValueEntry = Arbeidstilsynet.Common.Altinn.Storage.Models.KeyValueEntry;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Mapping;

/// <summary>
/// Maps the generated storage models onto the package's own models.
/// </summary>
/// <remarks>
/// The generated models are internal because <c>System.Text.Json</c> cannot serialise them
/// faithfully: enums come out as integers and free-form maps such as <c>dataValues</c> gain an
/// extra <c>additionalData</c> level. The models mapped to here are plain annotated POCOs, so a
/// consumer can return them from its own API unchanged.
/// </remarks>
internal static class StorageMappings
{
    public static AltinnInstance ToAltinnInstance(this GeneratedInstance source)
    {
        return new AltinnInstance
        {
            Id = source.Id,
            AppId = source.AppId,
            Org = source.Org,
            InstanceOwner = source.InstanceOwner is { } owner
                ? new InstanceOwner
                {
                    PartyId = owner.PartyId,
                    PersonNumber = owner.PersonNumber,
                    OrganisationNumber = owner.OrganisationNumber,
                    Username = owner.Username,
                }
                : null,
            Process = source.Process is { } process
                ? new ProcessState
                {
                    Started = process.Started?.DateTime,
                    StartEvent = process.StartEvent,
                    Ended = process.Ended?.DateTime,
                    EndEvent = process.EndEvent,
                }
                : null,
            CompleteConfirmations = source.CompleteConfirmations is { } confirmations
                ?
                [
                    .. confirmations.Select(confirmation => new CompleteConfirmation
                    {
                        StakeholderId = confirmation.StakeholderId,
                        ConfirmedOn = confirmation.ConfirmedOn?.DateTime,
                    }),
                ]
                : null,
            Data = source.Data is { } data ? [.. data.Select(ToDataElement)] : null,
            DataValues = ToStringDictionary(source.DataValues?.AdditionalData),
        };
    }

    public static AltinnQueryResponse<AltinnInstance> ToAltinnQueryResponse(
        this GeneratedInstanceQueryResponse source
    )
    {
        return new AltinnQueryResponse<AltinnInstance>
        {
            Count = source.Count,
            Self = source.Self,
            Next = source.Next,
            Instances = [.. (source.Instances ?? []).Select(ToAltinnInstance)],
        };
    }

    private static DataElement ToDataElement(GeneratedDataElement source)
    {
        return new DataElement
        {
            Id = source.Id,
            InstanceGuid = source.InstanceGuid,
            DataType = source.DataType,
            Filename = source.Filename,
            ContentType = source.ContentType,
            Size = source.Size,
            ContentHash = source.ContentHash,
            IsRead = source.IsRead,
            Tags = source.Tags ?? [],
            UserDefinedMetadata = ToStringDictionary(source.UserDefinedMetadata),
            Metadata = ToStringDictionary(source.Metadata),
            FileScanResult = source.FileScanResult switch
            {
                GeneratedFileScanResult.Clean => FileScanResult.Clean,
                GeneratedFileScanResult.Infected => FileScanResult.Infected,
                GeneratedFileScanResult.Pending => FileScanResult.Pending,
                GeneratedFileScanResult.NotApplicable => FileScanResult.NotApplicable,
                _ => null,
            },
        };
    }

    /// <summary>
    /// Flattens the generated key/value list into a dictionary.
    /// </summary>
    private static Dictionary<string, string> ToStringDictionary(
        List<GeneratedKeyValueEntry>? entries
    )
    {
        if (entries is null)
        {
            return [];
        }

        return entries
            .Where(entry => entry.Key is not null)
            .ToDictionary(entry => entry.Key!, entry => entry.Value ?? string.Empty);
    }

    /// <summary>
    /// Flattens one of Kiota's free-form maps, whose entries live in an untyped
    /// <c>AdditionalData</c> bag, into a plain dictionary.
    /// </summary>
    private static Dictionary<string, string> ToStringDictionary(
        IDictionary<string, object>? additionalData
    )
    {
        if (additionalData is null)
        {
            return [];
        }

        return additionalData
            .Where(entry => entry.Value is not null)
            .ToDictionary(entry => entry.Key, entry => entry.Value.ToString() ?? string.Empty);
    }
}
