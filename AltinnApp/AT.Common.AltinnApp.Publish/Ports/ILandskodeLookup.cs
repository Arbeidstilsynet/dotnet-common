using Arbeidstilsynet.Common.AltinnApp.Model;

namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Interface for looking up country codes and names based on 3-letter ISO values.
/// </summary>
public interface ILandskodeLookup
{
    /// <summary>
    /// Get a country code and name based on a 3-letter ISO code.
    /// </summary>
    /// <param name="alpha3Code">3-letter ISO code for the country</param>
    /// <returns>The Landskode matching the given ISO code, or null if not found.</returns>
    Task<Landskode?> GetLandskode(string alpha3Code);

    /// <summary>
    /// Get all country codes and names.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<KeyValuePair<string, Landskode>>> GetLandskoder();
}
