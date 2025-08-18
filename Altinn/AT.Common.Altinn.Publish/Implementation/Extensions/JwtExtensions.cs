using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Arbeidstilsynet.Common.Altinn.Implementation;

internal static class JwtExtensions
{
    public static string GenerateJwtGrant(
        string audience,
        string certificatePrivateKey,
        string certificateChain,
        string integrationId,
        string[] scopes
    )
    {
        var privateKey = Convert.FromBase64String(certificatePrivateKey);
        var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKey, out _);
        var signingCredentials = new SigningCredentials(
            new RsaSecurityKey(rsa),
            SecurityAlgorithms.RsaSha256
        );
        var claims = new ClaimsIdentity([new Claim("scope", string.Join(" ", scopes))]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(2), // Set token expiration
            Issuer = integrationId,
            Audience = audience,
            SigningCredentials = signingCredentials,
            AdditionalHeaderClaims = new Dictionary<string, object>
            {
                {
                    "x5c",
                    new List<string> { certificateChain }
                },
            },
        };
        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }
}
