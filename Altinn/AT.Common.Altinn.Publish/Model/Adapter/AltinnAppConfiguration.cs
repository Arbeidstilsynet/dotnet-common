namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

public record AltinnAppConfiguration(string? MainDocumentDataTypeName = null)
{
    public string MainDocumentDataTypeName { get; init; } = MainDocumentDataTypeName ?? "skjema";
}
