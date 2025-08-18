using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Model.Api;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

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
