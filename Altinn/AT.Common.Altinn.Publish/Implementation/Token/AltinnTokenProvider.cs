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
    private MaskinportenTokenResponse? _currentToken;

    private DateTime _lastUpdate = DateTime.Now;

    public async Task<string> GetToken()
    {
        if (_currentToken != null)
        {
            var timeDiff = DateTime.Now - _lastUpdate;
            if (timeDiff.Seconds >= _currentToken.ExpiresIn)
            { // get maskinporten token
                _currentToken = await maskinportenClient.GetToken();
                _lastUpdate = DateTime.Now;
            }
        }
        else
        {
            // get maskinporten token
            _currentToken = await maskinportenClient.GetToken();
            _lastUpdate = DateTime.Now;
        }
        // get altinn token
        return await altinnAuthenticationClient.ExchangeToken(
            Model.Api.AuthenticationTokenProvider.Maskinporten,
            _currentToken.AccessToken
        );
    }
}
