namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Interface for providing the organisasjonsnummer from a submitter for a given model.
/// </summary>
/// <typeparam name="TDataModel">The skjema data model type related to the instance.</typeparam>
public interface ISubmittersOrganisasjonsnummerProvider<TDataModel>
    where TDataModel : class
{
    /// <summary>
    /// Returns the organisasjonsnummer from the submitter for the given model.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    string GetOrganisasjonsnummerFromSubmitter(TDataModel model);
}
