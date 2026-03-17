using System.Security.Cryptography;
using System.Text;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class AltinnTokenClientTests
{
    private readonly VerifySettings _verifySettings = new();

    public AltinnTokenClientTests()
    {
        _verifySettings.UseDirectory("TestData/Snapshots");
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
        await Verifier
            .Verify(handler.ReadJsonWebToken(result), _verifySettings)
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
        await Verifier
            .Verify(handler.ReadJsonWebToken(result), _verifySettings)
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
}
