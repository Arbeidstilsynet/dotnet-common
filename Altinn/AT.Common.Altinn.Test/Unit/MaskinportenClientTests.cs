using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Model.Api;
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
    private readonly StubHandler _handler = new();
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

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory
            .CreateClient(MaskinportenApiClientKey)
            .Returns(_ => new HttpClient(_handler)
            {
                BaseAddress = new Uri("https://test.maskinporten.no/"),
            });

        _sut = new MaskinportenClient(httpClientFactory, Options.Create(configuration));
    }

    [Fact]
    public async Task GetToken_RequestsATokenUsingTheJwtBearerGrant()
    {
        _handler.RespondWith(NewToken(expiresIn: 120));

        var token = await _sut.GetToken();

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

        var first = await _sut.GetToken();
        var second = await _sut.GetToken();

        second.AccessToken.ShouldBe(first.AccessToken);
        _handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetToken_RequestsANewTokenOnceTheCachedOneIsWithinTheExpiryGrace()
    {
        // The client treats a token as expired 60 seconds early, so a 60 second lifetime is never
        // served from the cache.
        _handler.RespondWith(NewToken(expiresIn: 60, accessToken: "access-token-1"));
        var first = await _sut.GetToken();

        _handler.RespondWith(NewToken(expiresIn: 60, accessToken: "access-token-2"));
        var second = await _sut.GetToken();

        first.AccessToken.ShouldBe("access-token-1");
        second.AccessToken.ShouldBe("access-token-2");
        _handler.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetToken_IsSafeForConcurrentCallers()
    {
        _handler.RespondWith(NewToken(expiresIn: 120));

        var tokens = await Task.WhenAll(
            Enumerable.Range(0, 10).Select(_ => _sut.GetToken())
        );

        tokens.ShouldAllBe(token => token.AccessToken == "access-token-1");

        // The semaphore must collapse the burst into a single token request.
        _handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetToken_Throws_WhenMaskinportenRejectsTheRequest()
    {
        _handler.RespondWith(HttpStatusCode.BadRequest, """{"error":"invalid_grant"}""");

        await Should.ThrowAsync<Exception>(() => _sut.GetToken());
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
