using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

/// <summary>
/// Specification for an Altinn application
/// </summary>
/// <param name="AppId">e.g. ulykkesvarsel. Any organization prefix is removed.</param>
public record AltinnAppSpecification(string AppId)
{
    /// <summary>
    /// The application identifier of the Altinn application.
    /// </summary>
    /// <remarks>Any organization prefix is removed.</remarks>
    public string AppId { get; } =
        AppId.SanitizeAppId()
        ?? throw new ArgumentException("AppId cannot be null or empty", nameof(AppId));

    /// <summary>
    /// The <see cref="DataElement.DataType"/> of the main PDF document in <see cref="AltinnInstance.Data"/>. Defaults to "ref-data-as-pdf".
    /// </summary>
    public string MainPdfDataTypeId { get; init; } = "ref-data-as-pdf";

    /// <summary>
    /// The <see cref="FileMetadata.Filename"/> of the main PDF document. This will be used instead of <see cref="DataElement.Filename"/> in the <see cref="AltinnDocument"/>.
    /// </summary>
    public string MainPdfFileName { get; init; } = "main-document.pdf";

    /// <summary>
    /// The <see cref="DataElement.DataType"/> of the structured data (if any) in <see cref="AltinnInstance.Data"/>. Defaults to "structured-data".
    /// </summary>
    /// <remarks>Use the AddStructuredData extension in order to facilitate this feature</remarks>
    public string StructuredDataTypeId { get; init; } = "structured-data";

    /// <summary>
    /// The <see cref="FileMetadata.Filename"/> of the structured data (if any). This will be used instead of <see cref="DataElement.Filename"/> in the <see cref="AltinnDocument"/>.
    /// </summary>
    public string StructuredDataFileName { get; init; } = "structured-data.json";
}
