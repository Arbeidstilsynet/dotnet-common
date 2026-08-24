using Arbeidstilsynet.Common.Altinn.Storage.Models;

namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

public record FileMetadata
{
    public required Guid AltinnId { get; init; }
    public string? AltinnDataType { get; init; }
    public FileScanResult? FileScanResult { get; init; }
    public string? ContentType { get; init; }
    public string? Filename { get; init; }
}
