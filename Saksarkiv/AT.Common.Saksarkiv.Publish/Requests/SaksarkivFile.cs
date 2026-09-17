namespace Arbeidstilsynet.Common.Saksarkiv.V3;

/// <summary>
/// A file to upload as part of a Saksarkiv v3 request (for example a case document or attachment).
/// </summary>
/// <remarks>
/// The <see cref="FileName"/> is used both as the multipart part name and the file name, and must
/// match the file names referenced in the request payload (for example
/// <c>hoveddokumentFilnavn</c> / <c>vedleggFilnavn</c>). File names must be unique within a single
/// request.
/// </remarks>
public sealed record SaksarkivFile
{
    /// <summary>
    /// The file name, referenced from the request payload and used as the multipart part name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// The file content stream.
    /// </summary>
    public required Stream Content { get; init; }

    /// <summary>
    /// The MIME content type of the file. Defaults to <c>application/octet-stream</c>.
    /// </summary>
    public string ContentType { get; init; } = "application/octet-stream";
}
