using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.TestExtensions.Snapshots;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class AltinnTokenClientTests
{
    private readonly SnapshotSettings _snapshotSettings = new();

    public AltinnTokenClientTests()
    {
        _snapshotSettings
            .UseDirectory("TestData/Snapshots")
            .ScrubMembers(
                "exp",
                "iat",
                "nbf",
                "EncodedPayload",
                "EncodedSignature",
                "EncodedHeader",
                "EncodedToken",
                "ValidTo",
                "ValidFrom"
            );
    }

    [Fact]
    public async Task JwtExtensions_GenerateJwtGrant_MapsToCorrectFields()
    {
        //arrange
        using RSA rsa = RSA.Create();
        rsa.KeySize = 2048;
        // Export the private key
        var privateKey = rsa.ExportRSAPrivateKey();

        //act
        var result = JwtExtensions.GenerateJwtGrantWithCertificateChain(
            "https://test.maskinporten.no",
            Convert.ToBase64String(privateKey),
            "testChain",
            Guid.NewGuid().ToString(),
            ["test:read"]
        );
        //assert
        var handler = new JsonWebTokenHandler();
        await Snapshot.Verify(handler.ReadJsonWebToken(result), _snapshotSettings);
    }

    [Fact]
    public async Task JwtExtensions_GenerateTestJwtGrantWithPemSecret_MapsToCorrectFields()
    {
        //arrange
        using RSA rsa = RSA.Create();
        rsa.KeySize = 2048;
        // Export the private key
        var privateKey = rsa.ExportRSAPrivateKeyPem();
        //act
        var result = JwtExtensions.GenerateJwtGrantWithKey(
            "https://test.maskinporten.no/",
            Convert.ToBase64String(Encoding.UTF8.GetBytes(privateKey)),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            ["test:read"]
        );
        //assert
        var handler = new JsonWebTokenHandler();
        await Snapshot.Verify(handler.ReadJsonWebToken(result), _snapshotSettings);
    }

    [Fact]
    public void JwtExtensions_GenerateJwtGrant_AssignsAUniqueJwtId()
    {
        using RSA rsa = RSA.Create(2048);
        var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

        var first = JwtExtensions.GenerateJwtGrantWithKey(
            "https://test.maskinporten.no/",
            privateKey,
            "test-key",
            "test-integration",
            ["test:read"]
        );
        var second = JwtExtensions.GenerateJwtGrantWithKey(
            "https://test.maskinporten.no/",
            privateKey,
            "test-key",
            "test-integration",
            ["test:read"]
        );

        var handler = new JsonWebTokenHandler();
        var firstToken = handler.ReadJsonWebToken(first);
        var secondToken = handler.ReadJsonWebToken(second);

        firstToken.Id.ShouldNotBeNullOrWhiteSpace();
        secondToken.Id.ShouldNotBeNullOrWhiteSpace();
        secondToken.Id.ShouldNotBe(firstToken.Id);
        second.ShouldNotBe(first);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void JwtExtensions_GenerateJwtGrantWithJsonWebKey_CreatesSignedToken(
        bool base64EncodeJson
    )
    {
        // arrange
        using RSA rsa = RSA.Create(2048);
        var parameters = rsa.ExportParameters(true);
        var jsonWebKey = JsonSerializer.Serialize(
            new
            {
                alg = "RS256",
                d = Base64UrlEncoder.Encode(parameters.D),
                dp = Base64UrlEncoder.Encode(parameters.DP),
                dq = Base64UrlEncoder.Encode(parameters.DQ),
                e = Base64UrlEncoder.Encode(parameters.Exponent),
                kid = "test-key",
                kty = "RSA",
                n = Base64UrlEncoder.Encode(parameters.Modulus),
                p = Base64UrlEncoder.Encode(parameters.P),
                q = Base64UrlEncoder.Encode(parameters.Q),
                qi = Base64UrlEncoder.Encode(parameters.InverseQ),
                use = "sig",
            }
        );
        var privateKey = base64EncodeJson
            ? Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonWebKey))
            : jsonWebKey;

        // act
        var result = JwtExtensions.GenerateJwtGrantWithKey(
            "https://test.maskinporten.no/",
            privateKey,
            "test-key",
            Guid.NewGuid().ToString(),
            ["test:read"]
        );

        // assert
        var token = new JsonWebTokenHandler().ReadJsonWebToken(result);
        token.Kid.ShouldBe("test-key");
        token.Alg.ShouldBe(SecurityAlgorithms.RsaSha256);
    }
}
