using System.IdentityModel.Tokens.Jwt;

namespace Arbeidstilsynet.Common.Altinn.Ports.Adapter;

/// <summary>
/// Adapter interface for exchanging tokens.
/// </summary>
public interface IAltinnTokenAdapter
{
    /// <summary>
    /// Starts an token exchange via maskinporten.
    /// </summary>
    /// <param name="certificatePrivateKey">The private key from the client cert</param>
    /// <param name="integrationId">Integration which issues the client cert</param>
    /// <param name="scopes">Requested scopes</param>
    /// <returns>A valid jwt token to be used to query altinn 3 apis.</returns>
    public Task<string> StartTokenExchange(
        string certificatePrivateKey,
        string integrationId,
        string[] scopes
    );
}
