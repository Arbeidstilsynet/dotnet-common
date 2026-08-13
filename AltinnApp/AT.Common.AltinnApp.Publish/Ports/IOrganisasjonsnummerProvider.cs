namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Interface for providing the organisasjonsnummer for a given model.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IOrganisasjonsnummerProvider<T>
    where T : class
{
    /// <summary>
    /// Returns the organisasjonsnummer for the given model.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    string GetOrganisasjonsnummer(T model);
}
