using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using GeneratedAppsInstance = Arbeidstilsynet.Common.Altinn.Apps.Models.Instance;
using GeneratedDataElement = Arbeidstilsynet.Common.Altinn.Apps.Models.DataElement;
using GeneratedFileScanResult = Arbeidstilsynet.Common.Altinn.Apps.Models.FileScanResult;
using GeneratedKeyValueEntry = Arbeidstilsynet.Common.Altinn.Apps.Models.KeyValueEntry;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Mapping;

/// <summary>
/// Maps the generated apps models onto the package's own models.
/// </summary>
/// <remarks>
/// The apps API declares its own instance schema, separate from the storage API's, so it generates
/// a distinct type despite describing the same resource.
/// </remarks>
internal static class AppsMappings
{
    public static AltinnInstance ToAltinnInstance(this GeneratedAppsInstance source)
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
            Data = source.Data is { } data ? [.. data.Select(ToDataElement)] : null,
            DataValues = ToStringDictionary(source.DataValues?.AdditionalData),
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

    private static Dictionary<string, string> ToStringDictionary(
        List<GeneratedKeyValueEntry>? entries
    ) =>
        entries
            ?.Where(entry => entry.Key is not null)
            .ToDictionary(entry => entry.Key!, entry => entry.Value ?? string.Empty)
        ?? [];

    private static Dictionary<string, string> ToStringDictionary(
        IDictionary<string, object>? additionalData
    ) =>
        additionalData
            ?.Where(entry => entry.Value is not null)
            .ToDictionary(entry => entry.Key, entry => entry.Value.ToString() ?? string.Empty)
        ?? [];
}
