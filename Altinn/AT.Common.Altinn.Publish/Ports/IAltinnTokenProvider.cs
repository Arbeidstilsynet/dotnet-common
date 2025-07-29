namespace Arbeidstilsynet.Common.Altinn.Ports;

/// <summary>
/// Provides access to Altinn authentication tokens for API requests.
/// </summary>
public interface IAltinnTokenProvider
{
    /// <summary>
    /// Gets an authentication token for Altinn API requests.
    /// </summary>
    /// <returns>A valid Altinn API token as a string.</returns>
    Task<string> GetToken();
}
