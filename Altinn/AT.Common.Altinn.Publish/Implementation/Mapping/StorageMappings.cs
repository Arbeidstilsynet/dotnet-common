using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using GeneratedDataElement = Arbeidstilsynet.Common.Altinn.Storage.Models.DataElement;
using GeneratedFileScanResult = Arbeidstilsynet.Common.Altinn.Storage.Models.FileScanResult;
using GeneratedInstance = Arbeidstilsynet.Common.Altinn.Storage.Models.Instance;
using GeneratedInstanceQueryResponse = Arbeidstilsynet.Common.Altinn.Storage.Models.InstanceQueryResponse;

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
            DueBefore = source.DueBefore?.DateTime,
            VisibleAfter = source.VisibleAfter?.DateTime,
            InstanceOwner = source.InstanceOwner is { } owner
                ? new InstanceOwner
                {
                    PartyId = owner.PartyId,
                    PersonNumber = owner.PersonNumber,
                    OrganisationNumber = owner.OrganisationNumber,
                    Username = owner.Username,
                }
                : null!,
            Process = source.Process is { } process
                ? new ProcessState
                {
                    Started = process.Started?.DateTime,
                    StartEvent = process.StartEvent,
                    Ended = process.Ended?.DateTime,
                    EndEvent = process.EndEvent,
                }
                : null!,
            Data = [.. (source.Data ?? []).Select(ToDataElement)],
            DataValues = ToStringDictionary(source.DataValues?.AdditionalData),
            PresentationTexts = ToStringDictionary(source.PresentationTexts?.AdditionalData),
        };
    }

    public static AltinnQueryResponse<AltinnInstance> ToAltinnQueryResponse(
        this GeneratedInstanceQueryResponse source
    )
    {
        return new AltinnQueryResponse<AltinnInstance>
        {
            Count = source.Count ?? 0,
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
            BlobStoragePath = source.BlobStoragePath,
            Size = source.Size ?? 0,
            ContentHash = source.ContentHash,
            Locked = source.Locked ?? false,
            IsRead = source.IsRead ?? true,
            Tags = source.Tags ?? [],
            FileScanResult = source.FileScanResult switch
            {
                GeneratedFileScanResult.Clean => FileScanResult.Clean,
                GeneratedFileScanResult.Infected => FileScanResult.Infected,
                GeneratedFileScanResult.Pending => FileScanResult.Pending,
                _ => FileScanResult.NotApplicable,
            },
        };
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
