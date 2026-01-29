using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

/// <summary>
/// A summary of an Altinn instance. An Altinn instance represents a submitted form in Altinn.
/// </summary>
public record AltinnInstanceSummary
{
    /// <summary>
    /// Metadata about the instance.
    /// </summary>
    public required AltinnMetadata Metadata { get; init; }
    /// <summary>
    /// The PDF representation of the form. This is identified by <see cref="AltinnAppSpecification.MainPdfDataTypeId"/>
    /// </summary>
    public required AltinnDocument SkjemaAsPdf { get; init; }
    
    /// <summary>
    /// The structured data of the instance. This is identified by <see cref="AltinnAppSpecification.StructuredDataTypeId"/>
    /// </summary>
    public AltinnDocument? StructuredData { get; init; }
    
    /// <summary>
    /// Any <see cref="DataElement"/> that does not match either the PDF or structured data are treated as attachments.
    /// </summary>
    public required List<AltinnDocument> Attachments { get; init; } = [];
}
