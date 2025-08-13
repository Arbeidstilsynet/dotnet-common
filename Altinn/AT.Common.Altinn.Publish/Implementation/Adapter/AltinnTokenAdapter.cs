using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Altinn.App.Core.Features.Maskinporten;
using Altinn.App.Core.Models;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.IdentityModel.Tokens;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

internal class AltinnTokenAdapter(
    Ports.Clients.IMaskinportenClient maskinportenClient,
    IAltinnAuthenticationClient altinnAuthenticationClient
) : IAltinnTokenAdapter
{
    public async Task<string> StartTokenExchange(
        string certificatePrivateKey,
        string integrationId,
        string[] scopes
    )
    {
        // generate jwtGrant
        var jwtGrant = GenerateJwtGrant(
            maskinportenClient.BaseUrl().ToString(),
            certificatePrivateKey,
            integrationId,
            scopes
        );
        // get maskinporten token
        var maskinportenToken = await maskinportenClient.GetToken(jwtGrant);
        // get altinn token
        return await altinnAuthenticationClient.ExchangeToken(
            Model.Api.AuthenticationTokenProvider.Maskinporten,
            maskinportenToken.AccessToken
        );
    }

    internal static string GenerateJwtGrant(
        string audience,
        string certificatePrivateKey,
        string integrationId,
        string[] scopes
    )
    {
        var privateKey = Convert.FromBase64String(certificatePrivateKey);
        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKey, out _);
        var signingCredentials = new SigningCredentials(
            new RsaSecurityKey(rsa),
            SecurityAlgorithms.RsaSha256
        );
        var claims = new ClaimsIdentity(scopes.Select(s => new Claim("scope", s)));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(2), // Set token expiration
            Issuer = integrationId,
            Audience = audience,
            SigningCredentials = signingCredentials,
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
