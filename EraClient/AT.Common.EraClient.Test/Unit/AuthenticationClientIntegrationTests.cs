using Arbeidstilsynet.Common.EraClient.Ports;
using Arbeidstilsynet.Common.EraClient.Ports.Model;
using Arbeidstilsynet.Common.EraClient.Test.Fixtures;
using Shouldly;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.EraClient.Test;

public class AuthenticationClientIntegrationTests : TestBed<EraClientFixture>
{
    private readonly IAuthenticationClient _sut;

    private static AuthenticationRequestDto SampleRequest = new AuthenticationRequestDto
    {
        ClientId = "TestClientId",
        ClientSecret = "TestClientSecret",
    };

    private static AuthenticationResponseDto SampleResponse = new AuthenticationResponseDto
    {
        AccessToken = "TestAccessToken",
        TokenType = "Bearer",
        ExpiresIn = 3600,
    };

    private new readonly EraClientFixture _fixture;

    public AuthenticationClientIntegrationTests(
        ITestOutputHelper testOutputHelper,
        EraClientFixture fixture
    )
        : base(testOutputHelper, fixture)
    {
        _sut = fixture.GetService<IAuthenticationClient>(testOutputHelper)!;
        fixture
            .WireMockServer.Given(Request.Create().UsingPost())
            .RespondWith(Response.Create().WithStatusCode(401));
        _fixture = fixture;
    }

    [Fact]
    public async Task Authenticate_WhenCalledWithValidCredentials_ReturnsAccessToken()
    {
        //arrange
        _fixture
            .WireMockServer.Given(
                Request
                    .Create()
                    .UsingPost()
                    .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                    .WithBody(
                        new FormUrlEncodedMatcher(
                            [
                                "grant_type=client_credentials",
                                $"client_id={SampleRequest.ClientId}",
                                $"client_secret={SampleRequest.ClientSecret}",
                            ]
                        )
                    )
            )
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(200)
                    .WithBody(
                        $$"""
                        {
                            "access_token": "{{SampleResponse.AccessToken}}",
                            "token_type": "{{SampleResponse.TokenType}}",
                            "expires_in": {{SampleResponse.ExpiresIn}}
                        }
                        """
                    )
            );
        //act
        var result = await _sut.Authenticate(SampleRequest);
        //assert
        result.ShouldBe(SampleResponse);
    }

    [Fact]
    public async Task Authenticate_WhenCalledWithInvalidCredentials_ThrowsException()
    {
        //act
        var action = () =>
            _sut.Authenticate(
                new AuthenticationRequestDto { ClientId = "wrongId", ClientSecret = "wrongSecret" }
            );
        //assert
        await action.ShouldThrowAsync<HttpRequestException>();
    }
}
