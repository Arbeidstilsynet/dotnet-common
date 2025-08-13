using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Altinn.App.Core.Features.Maskinporten;
using Arbeidstilsynet.Common.Altinn.Implementation;
using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.IdentityModel.JsonWebTokens;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class AltinnTokenClientTests
{
    private readonly VerifySettings _verifySettings = new();

    public AltinnTokenClientTests()
    {
        _verifySettings.UseDirectory("TestData/Snapshots");
    }

    [Fact]
    public async Task AltinnTokenClientTests_GenerateJwtGrant_MapsToCorrectFields()
    {
        //arrange
        using RSA rsa = RSA.Create();
        rsa.KeySize = 2048;
        // Export the private key
        var privateKey = rsa.ExportRSAPrivateKey();

        //act
        var result = JwtExtensions.GenerateJwtGrant(
            "https://test.maskinporten.no",
            Convert.ToBase64String(privateKey),
            "testIntegration",
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
                "EncodedToken"
            );
    }
}
