using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

internal static class AltinnSpecificationExtensions
{
    internal const string StructuredDataTypeIdKey = "StructuredDataTypeId";
    internal const string MainPdfDataTypeId = "MainPdfDataTypeId";

    /// <summary>
    /// Gets the <see cref="AltinnAppSpecification"/> for the given <see cref="AltinnInstance"/>.
    /// </summary>
    /// <param name="AltinnInstance"></param>
    /// <returns>A default altinn specification, overridden by <see cref="AltinnInstance.DataValues"/> from the altinn AltinnInstance.</returns>
    /// <exception cref="ArgumentException">if the appId cannot be parsed from the AltinnInstance</exception>
    public static AltinnAppSpecification GetSpecification(this AltinnInstance AltinnInstance)
    {
        var sanitizedAppId =
            AltinnInstance.AppId.SanitizeAppId()
            ?? throw new ArgumentException(
                $"AppId '{AltinnInstance.AppId}' could not be sanitized to a valid format."
            );

        var resolvedSpec = new AltinnAppSpecification(sanitizedAppId);

        var dataValues = AltinnInstance.GetDataValues();

        if (
            dataValues.TryGetValue(StructuredDataTypeIdKey, out var val)
            && val is { Length: > 0 } structuredDataTypeId
        )
        {
            resolvedSpec = resolvedSpec with { StructuredDataTypeId = structuredDataTypeId };
        }

        if (
            dataValues.TryGetValue(MainPdfDataTypeId, out val)
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
            AltinnId = Guid.Parse(
                dataElement.Id ?? throw new ArgumentException("Data element has no id.")
            ),
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
    ) GetDataElementsBySignificance(this AltinnInstance AltinnInstance)
    {
        var appSpec = AltinnInstance.GetSpecification();

        var data = AltinnInstance.Data ?? [];

        var mainData =
            data.FirstOrDefault(d => d.DataType == appSpec.MainPdfDataTypeId)
            ?? throw new AltinnMainDataElementNotFoundException(
                AltinnInstance,
                appSpec.MainPdfDataTypeId,
                data.Select(d => d.DataType)
            );

        DataElement? structuredData = null;
        List<DataElement> attachmentData = [];

        foreach (var dataElement in data.Where(d => d.Id != mainData.Id))
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
