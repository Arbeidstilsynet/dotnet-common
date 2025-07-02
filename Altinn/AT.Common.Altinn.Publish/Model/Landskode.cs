namespace Arbeidstilsynet.Common.Altinn.Model;

/// <summary>
/// Landskode og navn på et gitt land
/// </summary>
/// <param name="Land">Leselig navn på engelsk. E.g. "Norway"</param>
/// <param name="Kode">E.g. "+47"</param>
public record Landskode(string Land, string Kode);