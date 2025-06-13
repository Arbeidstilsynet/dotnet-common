namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;

/// <summary>
/// Representerer paginering og sortering for søk i Enhetsregisteret.
/// </summary>
public record Pagination
{
    /// <summary>
    /// Antall resultater som skal hentes. Standard er 1000.
    /// </summary>
    public long Size { get; set; } = 1000;

    /// <summary>
    /// 0-basert sideindeks for paginering.
    /// </summary>
    public long Page { get; set; } = 0;
}
