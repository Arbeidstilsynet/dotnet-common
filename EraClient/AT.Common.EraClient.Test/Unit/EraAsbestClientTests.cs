using Arbeidstilsynet.Common.EraClient.Ports;
using Arbeidstilsynet.Common.EraClient.Ports.Model;
using Arbeidstilsynet.Common.EraClient.Ports.Model.Asbest;
using Arbeidstilsynet.Common.EraClient.Test.Fixtures;
using Bogus;
using Shouldly;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.EraClient.Test;

public class EraAsbestClientTests : TestBed<EraClientFixture>
{
    private readonly IEraAsbestClient _sut;

    private static string SampleOrgNr = "123123123";

    private static AuthenticationResponseDto SampleAuth = new AuthenticationResponseDto
    {
        AccessToken = "TestAccessToken",
        TokenType = "Bearer",
        ExpiresIn = 3600,
    };

    private static List<Melding> SampleMeldingerResponse = new Faker<Melding>()
        .UseSeed(9008)
        .Generate(5);

    private static SøknadStatusResponse SampleSøknadStatusResponse =
        new Faker<SøknadStatusResponse>().UseSeed(9008).Generate(1)[0];

    private new readonly EraClientFixture _fixture;

    public EraAsbestClientTests(ITestOutputHelper testOutputHelper, EraClientFixture fixture)
        : base(testOutputHelper, fixture)
    {
        _sut = fixture.GetService<IEraAsbestClient>(testOutputHelper)!;
        fixture
            .WireMockServer.Given(Request.Create().UsingPost())
            .RespondWith(Response.Create().WithStatusCode(401));
        _fixture = fixture;
    }

    [Fact]
    public async Task GetMeldingerByOrg_WhenCalledWithValidCredentials_ReturnsAccessToken()
    {
        //arrange
        _fixture
            .WireMockServer.Given(
                Request
                    .Create()
                    .UsingGet()
                    .WithHeader("Authorization", $"Bearer {SampleAuth.AccessToken}")
                    .WithPath($"/{SampleOrgNr}/meldinger")
            )
            .RespondWith(
                Response.Create().WithStatusCode(200).WithBodyAsJson(SampleMeldingerResponse)
            );
        //act
        var result = await _sut.GetMeldingerByOrg(SampleAuth, SampleOrgNr);
        //assert
        result.ShouldBeEquivalentTo(SampleMeldingerResponse);
    }

    [Fact]
    public async Task GetMeldingerByOrg_WhenCalledWithInvalidCredentials_ThrowsException()
    {
        //act
        var action = () =>
            _sut.GetMeldingerByOrg(
                new AuthenticationResponseDto
                {
                    AccessToken = "",
                    ExpiresIn = 0,
                    TokenType = "Bearer",
                },
                SampleOrgNr
            );
        //assert
        await action.ShouldThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetStatusForExistingSøknad_WhenCalledWithValidCredentials_ReturnsAccessToken()
    {
        //arrange
        _fixture
            .WireMockServer.Given(
                Request
                    .Create()
                    .UsingGet()
                    .WithHeader("Authorization", $"Bearer {SampleAuth.AccessToken}")
                    .WithPath($"/soknad/{SampleOrgNr}")
            )
            .RespondWith(
                Response.Create().WithStatusCode(200).WithBodyAsJson(SampleSøknadStatusResponse)
            );
        //act
        var result = await _sut.GetStatusForExistingSøknad(SampleAuth, SampleOrgNr);
        //assert
        result.ShouldBeEquivalentTo(SampleSøknadStatusResponse);
    }

    [Fact]
    public async Task GetStatusForExistingSøknad_WhenCalledWithInvalidCredentials_ThrowsException()
    {
        //act
        var action = () =>
            _sut.GetStatusForExistingSøknad(
                new AuthenticationResponseDto
                {
                    AccessToken = "",
                    ExpiresIn = 0,
                    TokenType = "Bearer",
                },
                SampleOrgNr
            );
        //assert
        await action.ShouldThrowAsync<HttpRequestException>();
    }
}
