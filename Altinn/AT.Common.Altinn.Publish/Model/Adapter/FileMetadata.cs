using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

public record FileMetadata
{
    public string? DataType { get; init; }
    public FileScanResult? FileScanResult { get; init; }
    public string? ContentType { get; init; }
    public string? Filename { get; init; }
}
