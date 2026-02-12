using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

internal static class AltinnSpecificationExtensions
{
    internal const string StructuredDataTypeIdKey = "StructuredDataTypeId";
    internal const string MainPdfDataTypeId = "MainPdfDataTypeId";

    /// <summary>
    /// Gets the <see cref="AltinnAppSpecification"/> for the given <see cref="AltinnInstance"/>.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns>A default altinn specification, overridden by <see cref="AltinnInstance.DataValues"/> from the altinn instance.</returns>
    /// <exception cref="ArgumentException">if the appId cannot be parsed from the instance</exception>
    public static AltinnAppSpecification GetSpecification(this AltinnInstance instance)
    {
        var sanitizedAppId =
            instance.AppId.SanitizeAppId()
            ?? throw new ArgumentException(
                $"AppId '{instance.AppId}' could not be sanitized to a valid format."
            );

        var resolvedSpec = new AltinnAppSpecification(sanitizedAppId);

        if (
            instance.DataValues.TryGetValue(StructuredDataTypeIdKey, out var val)
            && val is { Length: > 0 } structuredDataTypeId
        )
        {
            resolvedSpec = resolvedSpec with { StructuredDataTypeId = structuredDataTypeId };
        }

        if (
            instance.DataValues.TryGetValue(MainPdfDataTypeId, out val)
            && val is { Length: > 0 } mainPdfDataTypeId
        )
        {
            resolvedSpec = resolvedSpec with { MainPdfDataTypeId = mainPdfDataTypeId };
        }

        return resolvedSpec;
    }

    public static string? SanitizeAppId(this string? appId)
    {
        return appId?.Split('/').LastOrDefault() is { Length: > 0 } sanitizedAppId
            ? sanitizedAppId
            : null;
    }

    public static FileMetadata CreateFileMetadata(
        this AltinnAppSpecification appSpec,
        DataElement dataElement
    )
    {
        return new FileMetadata
        {
            AltinnId = Guid.Parse(dataElement.Id),
            ContentType = dataElement.ContentType,
            AltinnDataType = dataElement.DataType,
            Filename = appSpec.GetFilename(dataElement),
            FileScanResult = dataElement.FileScanResult,
        };
    }

    public static (
        DataElement mainData,
        DataElement? structuredData,
        IEnumerable<DataElement> attachmentData
    ) GetDataElementsBySignificance(this AltinnInstance instance)
    {
        var appSpec = instance.GetSpecification();

        var mainData =
            instance.Data.FirstOrDefault(d => d.DataType == appSpec.MainPdfDataTypeId)
            ?? throw new InvalidOperationException(
                $"Main document with data type '{appSpec.MainPdfDataTypeId}' not found in instance '{instance.Id}'. The instance was from app '{instance.AppId}'. Existing data types: [{string.Join(", ", instance.Data.Select(d => d.DataType))}]"
            );

        DataElement? structuredData = null;
        List<DataElement> attachmentData = [];

        foreach (var dataElement in instance.Data.Where(d => d.Id != mainData.Id))
        {
            if (dataElement.DataType == appSpec.StructuredDataTypeId)
            {
                structuredData = dataElement;
            }
            else
            {
                attachmentData.Add(dataElement);
            }
        }

        return (mainData, structuredData, attachmentData);
    }

    private static string GetFilename(this AltinnAppSpecification appSpec, DataElement dataElement)
    {
        if (appSpec.MainPdfDataTypeId == dataElement.DataType)
        {
            return appSpec.MainPdfFileName;
        }

        if (appSpec.StructuredDataTypeId == dataElement.DataType)
        {
            return appSpec.StructuredDataFileName;
        }

        return dataElement.Filename;
    }
}
