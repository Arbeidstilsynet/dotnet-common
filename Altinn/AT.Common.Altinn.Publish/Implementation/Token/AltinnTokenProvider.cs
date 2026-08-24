using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Token;

internal class AltinnTokenProvider(
    IMaskinportenClient maskinportenClient,
    IAltinnAuthenticationClient altinnAuthenticationClient
) : IAltinnTokenProvider
{
    public async Task<string> GetToken(CancellationToken cancellationToken = default)
    {
        var maskinportenToken = await maskinportenClient.GetToken(cancellationToken);

        // get altinn token
        return await altinnAuthenticationClient.ExchangeToken(
            maskinportenToken.AccessToken,
            cancellationToken: cancellationToken
        );
    }
}
