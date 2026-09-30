using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

/// <summary>
/// Contains metadata for an Altinn data element.
/// </summary>
public record FileMetadata
{
    /// <summary>
    /// Gets the data element identifier assigned by Altinn.
    /// </summary>
    public required Guid AltinnId { get; init; }

    /// <summary>
    /// Gets the Altinn data type.
    /// </summary>
    public string? AltinnDataType { get; init; }

    /// <summary>
    /// Gets the result of scanning the file.
    /// </summary>
    public FileScanResult? FileScanResult { get; init; }

    /// <summary>
    /// Gets the media type of the file.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets the file name.
    /// </summary>
    public string? Filename { get; init; }
}
