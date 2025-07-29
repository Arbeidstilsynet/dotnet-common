namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

public record AltinnDocument
{
    public required Stream DocumentContent { get; init; }

    /// <summary>
    /// The skjema which was sent in as XML
    /// </summary>
    public required bool IsMainDocument { get; init; }
    public required FileMetadata FileMetadata { get; init; }
}
