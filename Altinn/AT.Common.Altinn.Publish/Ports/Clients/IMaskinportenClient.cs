using Arbeidstilsynet.Common.Altinn.Model.Api;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for Maskinporten Authentication
/// </summary>
public interface IMaskinportenClient
{
    /// <summary>
    /// Get a Maskinporten token
    /// </summary>
    /// <returns></returns>
    /// <summary>
    /// Gets a Maskinporten token.
    /// </summary>
    /// <param name="cancellationToken">Cancels the token request.</param>
    Task<MaskinportenTokenResponse> GetToken(CancellationToken cancellationToken = default);
}
