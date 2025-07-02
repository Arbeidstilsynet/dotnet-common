using Arbeidstilsynet.Common.Altinn.Model;

namespace Arbeidstilsynet.Common.Altinn;

/// <summary>
/// Interface for å hente landskode og navn
/// </summary>
public interface ILandskodeLookup
{
    /// <summary>
    /// Hent landskode og navn
    /// </summary>
    /// <param name="landkode">3-bokstavs ISO-3166 </param>
    /// <returns></returns>
    Task<Landskode?> GetLandskode(string landkode);

    /// <summary>
    /// Hent alle landskoder og navn
    /// </summary>
    /// <returns>Alle land</returns>
    Task<IEnumerable<KeyValuePair<string, Landskode>>> GetLandskoder();
}