using Arbeidstilsynet.Common.Altinn.Authentication;
using Arbeidstilsynet.Common.Altinn.Model.Api;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Clients;

internal class AltinnAuthenticationClient(AuthenticationApiClient client)
    : IAltinnAuthenticationClient
{
    public async Task<string> ExchangeToken(
        string tokenProviderToken,
        AuthenticationTokenProvider tokenProvider = AuthenticationTokenProvider.Maskinporten,
        CancellationToken cancellationToken = default
    )
    {
        // This client is what mints Altinn tokens, so its request adapter authenticates
        // anonymously and the external provider's token is supplied per request instead.
        return await client
                .Exchange[tokenProvider.ToString().ToLowerInvariant()]
                .GetAsync(
                    request => request.Headers.Add("Authorization", $"Bearer {tokenProviderToken}"),
                    cancellationToken
                )
            ?? throw new InvalidOperationException("Failed to exchange token with Altinn");
    }
}
