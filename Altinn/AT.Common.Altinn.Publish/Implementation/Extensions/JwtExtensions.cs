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
    /// The private key is provided as a JWK or base64-encoded PEM or DER key, and the
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
    /// The private key is provided as a JWK or base64-encoded PEM or DER key, and the
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

    private static RSA ImportPrivateKey(string privateKey)
    {
        if (IsJsonWebKey(privateKey))
        {
            return ImportJsonWebKey(privateKey);
        }

        var keyBytes = Convert.FromBase64String(privateKey);
        var keyAsString = Encoding.UTF8.GetString(keyBytes);
        if (IsJsonWebKey(keyAsString))
        {
            return ImportJsonWebKey(keyAsString);
        }

        var rsa = RSA.Create();
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

    private static bool IsJsonWebKey(string privateKey) => privateKey.TrimStart().StartsWith('{');

    private static RSA ImportJsonWebKey(string json)
    {
        var jsonWebKey = new JsonWebKey(json);
        if (!string.Equals(jsonWebKey.Kty, JsonWebAlgorithmsKeyTypes.RSA, StringComparison.Ordinal))
        {
            throw new CryptographicException(
                $"Only RSA JSON Web Keys are supported, but the supplied key type was '{jsonWebKey.Kty}'."
            );
        }

        var rsaParameters = new RSAParameters
        {
            Modulus = DecodeRequiredParameter(jsonWebKey.N, "n"),
            Exponent = DecodeRequiredParameter(jsonWebKey.E, "e"),
            D = DecodeRequiredParameter(jsonWebKey.D, "d"),
            P = DecodeRequiredParameter(jsonWebKey.P, "p"),
            Q = DecodeRequiredParameter(jsonWebKey.Q, "q"),
            DP = DecodeRequiredParameter(jsonWebKey.DP, "dp"),
            DQ = DecodeRequiredParameter(jsonWebKey.DQ, "dq"),
            InverseQ = DecodeRequiredParameter(jsonWebKey.QI, "qi"),
        };

        var rsa = RSA.Create();
        rsa.ImportParameters(rsaParameters);
        return rsa;
    }

    private static byte[] DecodeRequiredParameter(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new CryptographicException(
                $"The RSA JSON Web Key is missing the required '{parameterName}' parameter."
            );
        }

        return Base64UrlEncoder.DecodeBytes(value);
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
        var claims = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim("scope", string.Join(" ", scopes)),
            ]
        );
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
