namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

/// <summary>
/// Represents a document from Altinn
/// </summary>
public record AltinnDocument
{
    /// <summary>
    /// The content of the document as a stream
    /// </summary>
    public required Stream DocumentContent { get; init; }
    /// <summary>
    /// Metadata about the document
    /// </summary>
    public required FileMetadata FileMetadata { get; init; }
}
