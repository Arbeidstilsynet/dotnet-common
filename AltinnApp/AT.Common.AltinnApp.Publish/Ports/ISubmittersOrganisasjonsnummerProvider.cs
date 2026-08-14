namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Interface for providing the organisasjonsnummer from a submitter for a given model.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISubmittersOrganisasjonsnummerProvider<T>
    where T : class
{
    /// <summary>
    /// Returns the organisasjonsnummer from the submitter for the given model.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    string GetOrganisasjonsnummerFromSubmitter(T model);
}
