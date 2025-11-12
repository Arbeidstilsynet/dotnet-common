namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

public record AltinnAppConfiguration(string? MainDocumentDataTypeName = null)
{
    /// <summary>
    /// The data type name of the main document in the Altinn application. Defaults to "structured-data".
    /// </summary>
    public string MainDocumentDataTypeName { get; init; } =
        MainDocumentDataTypeName ?? "structured-data";
}
