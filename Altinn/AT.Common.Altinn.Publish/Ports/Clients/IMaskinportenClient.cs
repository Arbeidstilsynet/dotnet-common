using Arbeidstilsynet.Common.Altinn.Model.Api;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for Maskinporten Authentication
/// </summary>
public interface IMaskinportenClient
{
    /// <summary>
    /// Gets a Maskinporten token for the given scopes.
    /// </summary>
    /// <param name="scopes">
    /// The scopes to request. Tokens are cached per distinct scope set, so callers asking for the
    /// same scopes share a token.
    /// </param>
    /// <param name="cancellationToken">Cancels the token request.</param>
    Task<MaskinportenTokenResponse> GetToken(
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default
    );
}
