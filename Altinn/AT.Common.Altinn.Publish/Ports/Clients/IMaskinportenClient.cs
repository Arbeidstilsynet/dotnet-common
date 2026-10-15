using Arbeidstilsynet.Common.Altinn.Model.Api;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;

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
    /// <exception cref="MaskinportenTokenException">
    /// Maskinporten returned an unsuccessful HTTP response. Inspect its status, OAuth error code,
    /// description, response body and requested scopes rather than parsing its message. Response
    /// details are untrusted and can contain sensitive data; select log-safe fields explicitly.
    /// </exception>
    Task<MaskinportenTokenResponse> GetToken(
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default
    );
}
