using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Token;

internal class AltinnTokenProvider(
    IMaskinportenClient maskinportenClient,
    IAltinnAuthenticationClient altinnAuthenticationClient
) : IAltinnTokenProvider
{
    public async Task<string> GetToken()
    {
        var maskinportenToken = await maskinportenClient.GetToken();

        // get altinn token
        return await altinnAuthenticationClient.ExchangeToken(maskinportenToken.AccessToken);
    }
}
