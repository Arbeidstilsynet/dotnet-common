using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Altinn.App.Core.Features.Maskinporten;
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

    private const string TestKey =
        "MIIEugIBADANBgkqhkiG9w0BAQEFAASCBKQwggSgAgEAAoIBAQC5nK1iFX4+hc0xx/N2O/6GMHkQ8+usLi2oHpXnuSh1smqszirWdjsXoChU1ASUCyXNNYsF0p6tx7d6nKhTsgjiW0JgIDFSpNffHvBqQPsaE0b86mJynjuqMZhneISAu74UUVX6lbn3wpX10B3Ko+w3jOHjpdNxS7k0pXOvJIMojNe9DsVH/T2OsZ0WgOHO4PK3ovi2FdaxkQrRUuHpHpRKvJRnQNsdZuSX1fawhNU1fQMsmqjFaAmM0hHu8R65SfptRVeVGL6KsMvg+YW2edKm35Ycab3JSdgzZ0IwHQgDRCROmS5CP14sJBjOxqhM0D5RtB27YbgCaP0otoAZUsGlAgMBAAECgf99yzG2x9pHhWcL1feqqf5V84QZeTa/+ov25MOzyEkje0fQ1ekyb+6clG7BDvALZYK5ERCnYZcpL7Gp54ItNvvObQsPGsJSwGWNUeqeGGwVv/rcbf1KsPK+5d3kQkmK9bJdEsKykcb3o2j+r42BSJdPDqwJHNmwbyGWDIvap2Q8lg/BZ+ZYqF8vrvlJFMY6CDOjIdH/gWfJtgO41A33jJXGy+A5CRhZhQ+u5+Nslz/mUrs+9zGZC7XoY+FtHCPzcSZIhNODmdPhL0Ziu/l81cV1qxXabrgvb9Q8OMHP9Pe+YB+JE23xkkmWQ4OByZGO4IhhGWeKDO3x9Qe34zfYRfMCgYEA5ajAD21BZ1WJ4PK9N7vGB6kzJRy3DaJdr3KlPgsJlB6mXQklBMJCrT645oV+7NgS/vv1zu3aZTDArBfQeqbjW2F52+qjq2RdBPejuzYhT1lmBbeLGs2Eqpd/iklWgHyMeQJ0UtApVqQc9BkuQ3vVBDs5QeA02Snk9H+W9qkML2MCgYEAzuada7UGByUnf7CYDHqtGUj25dnPzcpq+tK8uZB3C37j0wFIXwJxEuYSoQSwz0uZxtrGI4YMFJEJmEdHpLhGAwxIvUb34/Za7Su4HkcU/s9oh4JaeMH+s6BBvEca9SfHW8BqQfbhTtwOX1SBaXiIAOcRrelCT7pvacj3ysFw7VcCgYAsiSX9l91YEaF0Sv5goXxMngY6CzCAZ10tmdjriC5qV+tBHXXdT2KowAIRShAME/bTFb9cSonQl7y1YsTeFCEA5o1AFWd55DOZtAA/XMbm4VpEf4xtPS+d+VUVVxV8QyrmiJBQQufRUm/+8icjlQ8BDA8VdEorVtF8jIWU/cFMKwKBgHZ13b30WG4w3KukXXoPpxoqB4rUqcEG2zee/wUz7KT9Us3WFyymxjzu082zVNRrUbFkQzvFnRcNb7PrY96wzw0htGgCwR2ZSLgwZuuXATNfZ1bm0IwBbqZD5D87avjgfKlQwsXf52JyZhw40xujMiNqKVosipYBhZbIxO8m0FRjAoGAdhyf+yLKRVGiBf2heqMe5+VWR4Sw7I2qqH9fi6DMJIdp5uDH5qKBCKsTlHX2n6tJn717AX4gLUSiNjSKBybn2UeMm422k5r/BNClwDVXIMAVBrKAscO1hlCV6diiUhht5nlT6vm15VVz+yT8wA4u1Pj9/TuJMIGwG4BUtpNYNa0=";

    [Fact]
    public async Task AltinnTokenClientTests_GenerateJwtGrant_MapsToCorrectFields()
    {
        //arrange
        using RSA rsa = RSA.Create();
        rsa.KeySize = 2048;
        // Export the private key
        var privateKey = rsa.ExportRSAPrivateKey();

        //act
        var result = AltinnTokenAdapter.GenerateJwtGrant(
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
