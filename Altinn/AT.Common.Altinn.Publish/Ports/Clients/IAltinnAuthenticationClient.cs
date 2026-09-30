using Arbeidstilsynet.Common.Altinn.Model.Api;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for Authentication
/// </summary>
public interface IAltinnAuthenticationClient
{
    /// <summary>
    /// Exchanges a token from an external authentication provider for an Altinn token.
    /// </summary>
    /// <param name="tokenProviderToken">The token issued by the external provider.</param>
    /// <param name="tokenProvider">The external provider that issued the token.</param>
    /// <param name="cancellationToken">Cancels the exchange request.</param>
    /// <returns>An Altinn token.</returns>
    Task<string> ExchangeToken(
        string tokenProviderToken,
        AuthenticationTokenProvider tokenProvider = AuthenticationTokenProvider.Maskinporten,
        CancellationToken cancellationToken = default
    );
}
