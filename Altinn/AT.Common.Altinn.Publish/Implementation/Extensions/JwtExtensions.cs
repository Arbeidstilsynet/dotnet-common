using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Extensions;

internal static class JwtExtensions
{
    /// <summary>
    /// Generates a JWT grant using a pre-registered public key in Maskinporten.
    /// The private key is provided as a base64-encoded PEM or DER key, and the
    /// <paramref name="keyId"/> is used to identify the key (kid header).
    /// </summary>
    public static string GenerateJwtGrantWithKey(
        string audience,
        string privateKey,
        string keyId,
        string integrationId,
        string[] scopes
    )
    {
        var rsa = ImportPrivateKey(privateKey);
        var rsaKey = new RsaSecurityKey(rsa) { KeyId = keyId };
        return CreateToken(audience, integrationId, scopes, rsaKey, additionalHeaderClaims: null);
    }

    /// <summary>
    /// Generates a JWT grant using a certificate chain (x5c header).
    /// The private key is provided as a base64-encoded PEM or DER key, and the
    /// <paramref name="certificateChain"/> is included as the x5c JWT header.
    /// </summary>
    public static string GenerateJwtGrantWithCertificateChain(
        string audience,
        string privateKey,
        string certificateChain,
        string integrationId,
        string[] scopes
    )
    {
        var rsa = ImportPrivateKey(privateKey);
        var rsaKey = new RsaSecurityKey(rsa);
        var additionalHeaderClaims = new Dictionary<string, object>
        {
            {
                "x5c",
                new List<string> { certificateChain }
            },
        };
        return CreateToken(audience, integrationId, scopes, rsaKey, additionalHeaderClaims);
    }

    private static RSA ImportPrivateKey(string base64EncodedKey)
    {
        var rsa = RSA.Create();
        var keyBytes = Convert.FromBase64String(base64EncodedKey);
        var keyAsString = Encoding.UTF8.GetString(keyBytes);
        if (keyAsString.Contains("-----BEGIN", StringComparison.Ordinal))
        {
            rsa.ImportFromPem(keyAsString);
        }
        else
        {
            rsa.ImportRSAPrivateKey(keyBytes, out _);
        }

        return rsa;
    }

    private static string CreateToken(
        string audience,
        string integrationId,
        string[] scopes,
        RsaSecurityKey rsaKey,
        Dictionary<string, object>? additionalHeaderClaims
    )
    {
        var signingCredentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);
        var claims = new ClaimsIdentity([new Claim("scope", string.Join(" ", scopes))]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(2),
            Issuer = integrationId,
            Audience = audience,
            SigningCredentials = signingCredentials,
            AdditionalHeaderClaims = additionalHeaderClaims ?? [],
        };
        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }
}
