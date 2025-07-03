using Arbeidstilsynet.Common.Altinn.Model;

namespace Arbeidstilsynet.Common.Altinn.Ports;

/// <summary>
/// Interface for looking up country codes and names based on 3-letter ISO values.
/// </summary>
public interface ILandskodeLookup
{
    /// <summary>
    /// Get a country code and name based on a 3-letter ISO code.
    /// </summary>
    /// <param name="isoCode">3-letter ISO code for the country</param>
    /// <returns></returns>
    Task<Landskode?> GetLandskode(string isoCode);

    /// <summary>
    /// Get all country codes and names.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<KeyValuePair<string, Landskode>>> GetLandskoder();
}
