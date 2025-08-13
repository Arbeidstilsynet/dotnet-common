using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

internal class AltinnTokenProvider(
    IMaskinportenClient maskinportenClient,
    IAltinnAuthenticationClient altinnAuthenticationClient,
    IOptions<AltinnAuthenticationConfiguration> altinnAuthenticationConfiguration
) : IAltinnTokenProvider
{
    public async Task<string> GetToken()
    {
        var config = altinnAuthenticationConfiguration.Value;
        var jwtToken = await StartTokenExchange(
            config.CertificatePrivateKey,
            config.IntegrationId,
            config.Scopes
        );
        return jwtToken;
    }

    private async Task<string> StartTokenExchange(
        string certificatePrivateKey,
        string integrationId,
        string[] scopes
    )
    {
        // generate jwtGrant
        var jwtGrant = GenerateJwtGrant(
            maskinportenClient.BaseUrl.ToString(),
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
