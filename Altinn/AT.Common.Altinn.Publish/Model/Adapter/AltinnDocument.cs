using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

/// <summary>
/// Represents a <see cref="DataElement"/> from Altinn, containing the file content as a stream and associated metadata.
/// </summary>
public record AltinnDocument
{
    /// <summary>
    /// The content of the document as a stream. The stream should be readable and seekable.
    /// </summary>
    public required Stream DocumentContent { get; init; }
    
    /// <summary>
    /// Metadata about the file, such as file name and content type.
    /// </summary>
    public required FileMetadata FileMetadata { get; init; }
}
