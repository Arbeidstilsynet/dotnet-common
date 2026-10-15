using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Implementation.ErrorReporting;
using Arbeidstilsynet.Common.Altinn.Model.Api;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using static Arbeidstilsynet.Common.Altinn.DependencyInjection.DependencyInjectionExtensions;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

/// <summary>
/// Covers the Maskinporten client, which remains hand-written because Maskinporten publishes no
/// OpenAPI specification, and which owns the token cache every other client depends on.
/// </summary>
public class MaskinportenClientTests
{
    private static readonly string[] Scopes = ["altinn:serviceowner"];

    private readonly StubHandler _handler = new();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MaskinportenClient _sut;

    public MaskinportenClientTests()
    {
        using var rsa = RSA.Create(2048);

        var configuration = new MaskinportenConfiguration
        {
            PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey()),
            KeyId = "test-key-id",
            IntegrationId = "test-integration",
            Scopes = ["altinn:serviceowner"],
        };

        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _httpClientFactory
            .CreateClient(MaskinportenApiClientKey)
            .Returns(_ => new HttpClient(_handler)
            {
                BaseAddress = new Uri("https://test.maskinporten.no/"),
            });

        _sut = new MaskinportenClient(_httpClientFactory, Options.Create(configuration));
    }

    [Fact]
    public async Task GetToken_RequestsATokenUsingTheJwtBearerGrant()
    {
        _handler.RespondWith(NewToken(expiresIn: 120));

        var token = await _sut.GetToken(Scopes);

        token.AccessToken.ShouldBe("access-token-1");

        _handler.Requests.Count.ShouldBe(1);
        _handler.Requests[0].RequestUri!.ToString().ShouldBe("https://test.maskinporten.no/token");

        var body = _handler.RequestBodies[0];
        body.ShouldContain("grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Ajwt-bearer");
        body.ShouldContain("assertion=");
    }

    [Fact]
    public async Task GetToken_ReusesTheCachedTokenWhileItRemainsValid()
    {
        _handler.RespondWith(NewToken(expiresIn: 120));

        var first = await _sut.GetToken(Scopes);
        var second = await _sut.GetToken(Scopes);

        second.AccessToken.ShouldBe(first.AccessToken);
        _handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetToken_RequestsANewTokenOnceTheCachedOneIsWithinTheExpiryGrace()
    {
        // The client treats a token as expired 60 seconds early, so a 60 second lifetime is never
        // served from the cache.
        _handler.RespondWith(NewToken(expiresIn: 60, accessToken: "access-token-1"));
        var first = await _sut.GetToken(Scopes);

        _handler.RespondWith(NewToken(expiresIn: 60, accessToken: "access-token-2"));
        var second = await _sut.GetToken(Scopes);

        first.AccessToken.ShouldBe("access-token-1");
        second.AccessToken.ShouldBe("access-token-2");
        _handler.Requests.Count.ShouldBe(2);
        _httpClientFactory.Received(2).CreateClient(MaskinportenApiClientKey);
    }

    [Fact]
    public async Task GetToken_IsSafeForConcurrentCallers()
    {
        _handler.RespondWith(NewToken(expiresIn: 120));

        var tokens = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => _sut.GetToken(Scopes)));

        tokens.ShouldAllBe(token => token.AccessToken == "access-token-1");

        // The semaphore must collapse the burst into a single token request.
        _handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetToken_RequestsASeparateTokenPerScopeSet()
    {
        // Scopes are baked into the grant, so a token issued for one API must not be handed to a
        // client registered for different scopes.
        _handler.RespondWith(NewToken(expiresIn: 120, accessToken: "storage-token"));
        var storage = await _sut.GetToken(["altinn:serviceowner/instances.read"]);

        _handler.RespondWith(NewToken(expiresIn: 120, accessToken: "events-token"));
        var events = await _sut.GetToken(["altinn:events.subscribe"]);

        storage.AccessToken.ShouldBe("storage-token");
        events.AccessToken.ShouldBe("events-token");
        _handler.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetToken_ReusesTheCachedTokenWhenTheSameScopesAreRequestedInAnotherOrder()
    {
        _handler.RespondWith(NewToken(expiresIn: 120));

        var first = await _sut.GetToken(["b:scope", "a:scope"]);
        var second = await _sut.GetToken(["a:scope", "b:scope"]);

        second.AccessToken.ShouldBe(first.AccessToken);
        _handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetToken_Throws_WhenMaskinportenRejectsTheRequest()
    {
        _handler.RespondWith(
            HttpStatusCode.BadRequest,
            """{"error":"invalid_scope","error_description":"Scope not allowed"}"""
        );

        var exception = await Should.ThrowAsync<HttpRequestException>(() => _sut.GetToken(Scopes));

        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldContain("invalid_scope");
        exception.Message.ShouldContain("Scope not allowed");
    }

    [Fact]
    public async Task GetToken_ExposesStructuredRejectionWithoutParsingTheExceptionMessage()
    {
        _handler.RespondWith(
            HttpStatusCode.BadRequest,
            """{"error":"invalid_scope","error_description":"Scope not allowed"}"""
        );

        var exception = await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.GetToken(["altinn:events.subscribe"])
        );

        var structured = exception.ShouldBeOfType<MaskinportenTokenException>();
        structured.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        structured.ErrorCode.ShouldBe("invalid_scope");
        structured.ErrorDescription.ShouldBe("Scope not allowed");
        structured.ResponseBody.ShouldBe(
            """{"error":"invalid_scope","error_description":"Scope not allowed"}"""
        );
        structured.RequestedScopes.ShouldBe(["altinn:events.subscribe"]);
        structured.Message.ShouldContain("Scope not allowed");
    }

    [Theory]
    [InlineData("not JSON")]
    [InlineData("""{"error":123}""")]
    [InlineData("""{"error":null}""")]
    [InlineData("""{"error_description":"invalid_scope"}""")]
    [InlineData("""{"error":"invalid_scope" """)]
    [InlineData("""["invalid_scope"]""")]
    [InlineData("null")]
    public async Task GetToken_MalformedOAuthBody_StillExposesStatusAndScopes(string body)
    {
        _handler.RespondWith(HttpStatusCode.Unauthorized, body);

        var exception = await Should.ThrowAsync<MaskinportenTokenException>(() =>
            _sut.GetToken(Scopes)
        );

        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.ErrorCode.ShouldBeNull();
        exception.ResponseBody.ShouldBe(body);
        exception.RequestedScopes.ShouldBe(Scopes);
        exception.Message.ShouldContain(body);
    }

    [Fact]
    public async Task GetToken_PreservesProviderErrorWithoutSelectingTheLoggingPolicy()
    {
        _handler.RespondWith(
            HttpStatusCode.BadRequest,
            """{"error":"unrecognized-provider-code"}"""
        );

        var exception = await Should.ThrowAsync<MaskinportenTokenException>(() =>
            _sut.GetToken(Scopes)
        );

        exception.ErrorCode.ShouldBe("unrecognized-provider-code");
    }

    [Theory]
    [InlineData(
        """{"error":"invalid_scope","error_description":123,"extra":"provider-detail"}""",
        "invalid_scope",
        null
    )]
    [InlineData(
        """{"error":123,"error_description":"Detailed provider explanation"}""",
        null,
        "Detailed provider explanation"
    )]
    [InlineData(
        """{"error_description":"synthetic-token synthetic-private-key 12345678901"}""",
        null,
        "synthetic-token synthetic-private-key 12345678901"
    )]
    public async Task GetToken_ExposesProviderDetails_WithoutApplyingALoggingPolicy(
        string body,
        string? expectedCode,
        string? expectedDescription
    )
    {
        _handler.RespondWith(HttpStatusCode.BadRequest, body);

        var exception = await Should.ThrowAsync<MaskinportenTokenException>(() =>
            _sut.GetToken(Scopes)
        );

        exception.ErrorCode.ShouldBe(expectedCode);
        exception.ErrorDescription.ShouldBe(expectedDescription);
        exception.ResponseBody.ShouldBe(body);
    }

    [Fact]
    public async Task GetToken_FailureSnapshotsScopes_AndDoesNotCacheRejections()
    {
        string[] scopes = ["altinn:events.subscribe"];
        _handler.RespondWith(HttpStatusCode.BadRequest, """{"error":"invalid_scope"}""");
        var exception = await Should.ThrowAsync<MaskinportenTokenException>(() =>
            _sut.GetToken(scopes)
        );
        scopes[0] = "changed:scope";

        exception.RequestedScopes.ShouldBe(["altinn:events.subscribe"]);
        exception.RequestedScopes.ShouldBeAssignableTo<IList<string>>().IsReadOnly.ShouldBeTrue();
        _handler.RespondWith(NewToken(expiresIn: 120));
        await _sut.GetToken(["altinn:events.subscribe"]);
        _handler.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetToken_TruncatesLongMaskinportenErrorResponses()
    {
        _handler.RespondWith(
            HttpStatusCode.BadRequest,
            new string('x', ErrorResponseBodyCapture.MaxBodyLength * 10)
        );

        var exception = await Should.ThrowAsync<MaskinportenTokenException>(() =>
            _sut.GetToken(Scopes)
        );

        exception.Message.ShouldContain("(truncated)");
        exception.Message.Length.ShouldBeLessThan(ErrorResponseBodyCapture.MaxBodyLength + 200);
        exception.ErrorCode.ShouldBeNull();
        exception.ErrorDescription.ShouldBeNull();
        exception.ResponseBody.ShouldBe(
            new string('x', ErrorResponseBodyCapture.MaxBodyLength) + "... (truncated)"
        );
        exception.RequestedScopes.ShouldBe(Scopes);
    }

    [Fact]
    public async Task GetToken_Throws_WhenResponseHasNoAccessToken()
    {
        _handler.RespondWith("""{"token_type":"Bearer","expires_in":120}""");

        await Should.ThrowAsync<JsonException>(() => _sut.GetToken(Scopes));
    }

    [Fact]
    public async Task GetToken_AllowsResponseWithoutScope()
    {
        _handler.RespondWith(
            """{"access_token":"access-token","token_type":"Bearer","expires_in":120}"""
        );

        var result = await _sut.GetToken(Scopes);

        result.Scope.ShouldBeNull();
    }

    private static string NewToken(int expiresIn, string accessToken = "access-token-1") =>
        JsonSerializer.Serialize(
            new MaskinportenTokenResponse
            {
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresIn = expiresIn,
                Scope = "altinn:serviceowner",
            },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

    private sealed class StubHandler : HttpMessageHandler
    {
        private HttpStatusCode _statusCode = HttpStatusCode.OK;
        private string _body = "{}";

        public List<HttpRequestMessage> Requests { get; } = [];
        public List<string> RequestBodies { get; } = [];

        public void RespondWith(string body) => RespondWith(HttpStatusCode.OK, body);

        public void RespondWith(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _body = body;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            lock (Requests)
            {
                Requests.Add(request);
                RequestBodies.Add(
                    request.Content?.ReadAsStringAsync(cancellationToken).Result ?? string.Empty
                );
            }

            // Give concurrent callers a chance to arrive while the first request is in flight.
            await Task.Delay(10, cancellationToken);

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json"),
            };
        }
    }
}
