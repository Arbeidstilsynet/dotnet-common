using Altinn.Platform.Storage.Interface.Models;

namespace Arbeidstilsynet.Common.AltinnApp.Model;

/// <summary>
/// Context passed to <see cref="Ports.IStructuredDataPostProcessor"/> implementations.
/// </summary>
public record StructuredDataPostProcessingContext
{
    /// <summary>
    /// The instance, freshly fetched from storage.
    /// </summary>
    public required Instance Instance { get; init; }

    /// <summary>
    /// The structured data element. Its data type is <see cref="DependencyInjection.StructuredDataConfiguration.StructuredDataTypeId"/>.
    /// </summary>
    public required DataElement StructuredDataElement { get; init; }

    /// <summary>
    /// The main PDF element. Its data type is <see cref="DependencyInjection.StructuredDataConfiguration.MainPdfDataTypeId"/>.
    /// </summary>
    public required DataElement MainPdfElement { get; init; }
}
