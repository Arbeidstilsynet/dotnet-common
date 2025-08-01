namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

public record FileMetadata
{
    public string? DataType { get; init; }
    public FileScanResult? FileScanResult { get; init; }
    public string? ContentType { get; init; }
    public string? Filename { get; init; }
}

public enum FileScanResult
{
    /// <summary>
    /// The file will not be scanned. File scanning is turned off.
    /// </summary>
    NotApplicable,

    /// <summary>
    /// The scan status of the file is pending. This is the default value.
    /// </summary>
    Pending,

    /// <summary>
    /// The file scan did not find any malware in the file.
    /// </summary>
    Clean,

    /// <summary>
    /// The file scan found malware in the file.
    /// </summary>
    Infected,
}
