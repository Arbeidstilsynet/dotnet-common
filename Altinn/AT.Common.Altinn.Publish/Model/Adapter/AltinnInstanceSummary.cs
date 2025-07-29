namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

public record AltinnInstanceSummary
{
    public required AltinnMetadata Metadata { get; init; }
    public required AltinnDocument AltinnSkjema { get; init; }
    public required List<AltinnDocument> Attachments { get; init; } = [];
}
