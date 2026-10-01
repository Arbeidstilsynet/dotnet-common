using System.Net;
using System.Text;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Events;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Implementation.ErrorReporting;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Bundle;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

/// <summary>
/// Covers putting what Altinn answered into the message of the exceptions the clients throw, so
/// that it reaches logs without callers having to dig for it.
/// </summary>
public class AltinnErrorReportingTests
{
    private const string EventsBaseUrl = "https://platform.tt02.altinn.no/events/api/v1";

    private const string ProblemBody = """
        {
          "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
          "title": "Bad Request",
          "status": 400,
          "detail": "The subscription endpoint is not reachable."
        }
        """;

    private sealed class StubHandler(HttpStatusCode status, string body, string mediaType)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new HttpResponseMessage(status)
                {
                    Content = new StringContent(body, Encoding.UTF8, mediaType),
                }
            );
    }

    private sealed class ResponseHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => Task.FromResult(response);
    }

    private static AltinnEventsClient EventsClient(
        HttpStatusCode status,
        string body,
        bool capture = true,
        string mediaType = "application/problem+json"
    )
    {
        HttpMessageHandler handler = new StubHandler(status, body, mediaType);
        if (capture)
        {
            handler = new AltinnErrorResponseCaptureHandler { InnerHandler = handler };
        }

        var adapter = new DefaultRequestAdapter(
            new AnonymousAuthenticationProvider(),
            httpClient: new HttpClient(handler)
        )
        {
            BaseUrl = EventsBaseUrl,
        };

        return new AltinnEventsClient(
            new EventsApiClient(new AltinnErrorReportingRequestAdapter(adapter, "Events"))
        );
    }

    private static Task<AltinnSubscription> Subscribe(AltinnEventsClient client) =>
        client.Subscribe(new AltinnSubscriptionRequest { TypeFilter = "x" });

    [Fact]
    public async Task DeclaredProblem_MessageNamesRequestStatusAndBody()
    {
        var exception = await Should.ThrowAsync<ApiException>(() =>
            Subscribe(EventsClient(HttpStatusCode.BadRequest, ProblemBody))
        );

        exception.Message.ShouldStartWith(
            $"Altinn Events request POST {EventsBaseUrl}/subscriptions failed with status 400"
        );
        exception.Message.ShouldContain("The subscription endpoint is not reachable.");
        exception.ResponseStatusCode.ShouldBe(400);
    }

    [Fact]
    public async Task DeclaredProblem_IsStillReachableThroughGetAltinnProblemDetails()
    {
        var exception = await Should.ThrowAsync<ApiException>(() =>
            Subscribe(EventsClient(HttpStatusCode.BadRequest, ProblemBody))
        );

        var problem = exception.GetAltinnProblemDetails();

        problem.ShouldNotBeNull();
        problem.Status.ShouldBe(400);
        problem.Detail.ShouldBe("The subscription endpoint is not reachable.");
    }

    [Fact]
    public async Task UndeclaredStatus_MessageCarriesTheRawBody()
    {
        // The Events spec declares no error type for 500, so Kiota throws a bare ApiException and
        // discards the body; the capture handler is what keeps it.
        var exception = await Should.ThrowAsync<ApiException>(() =>
            Subscribe(
                EventsClient(
                    HttpStatusCode.InternalServerError,
                    "upstream exploded",
                    mediaType: "text/plain"
                )
            )
        );

        exception.Message.ShouldContain("failed with status 500");
        exception.Message.ShouldContain("upstream exploded");
        exception.ResponseStatusCode.ShouldBe(500);
    }

    [Fact]
    public async Task WithoutCapture_MessageFallsBackToTheParsedProblemDetails()
    {
        var exception = await Should.ThrowAsync<ApiException>(() =>
            Subscribe(EventsClient(HttpStatusCode.BadRequest, ProblemBody, capture: false))
        );

        exception.Message.ShouldContain("failed with status 400");
        exception.Message.ShouldContain("The subscription endpoint is not reachable.");
    }

    [Fact]
    public async Task LongBodies_AreTruncated()
    {
        var body = new string('x', AltinnErrorResponseCaptureHandler.MaxBodyLength + 500);

        var exception = await Should.ThrowAsync<ApiException>(() =>
            Subscribe(
                EventsClient(HttpStatusCode.InternalServerError, body, mediaType: "text/plain")
            )
        );

        exception.Message.ShouldContain("(truncated)");
        exception.Message.Length.ShouldBeLessThan(
            AltinnErrorResponseCaptureHandler.MaxBodyLength + 300
        );
    }

    [Fact]
    public async Task LongBodies_ReadOnlyTheDiagnosticPrefixAndRestoreTheFullResponse()
    {
        var body = new string('x', AltinnErrorResponseCaptureHandler.MaxBodyLength * 10);
        var stream = new CountingReadStream(Encoding.UTF8.GetBytes(body));
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StreamContent(stream),
        };
        var handler = new AltinnErrorResponseCaptureHandler
        {
            InnerHandler = new ResponseHandler(response),
        };
        var capture = new AltinnErrorResponseCapture();
        using var request = new HttpRequestMessage(HttpMethod.Get, EventsBaseUrl);
        request.Options.Set(AltinnErrorResponseCapture.Key, capture);
        using var invoker = new HttpMessageInvoker(handler);

        using var capturedResponse = await invoker.SendAsync(request, CancellationToken.None);

        stream.BytesRead.ShouldBe(AltinnErrorResponseCaptureHandler.MaxBodyLength + 1);
        capture.Body.ShouldEndWith("(truncated)");
        capture.Body!.Length.ShouldBeLessThan(AltinnErrorResponseCaptureHandler.MaxBodyLength + 30);
        (await capturedResponse.Content.ReadAsStringAsync()).ShouldBe(body);
    }

    [Fact]
    public async Task RegisteredClients_ReportWhatAltinnAnswered()
    {
        var tokenProvider = Substitute.For<IAltinnTokenProvider>();
        tokenProvider
            .GetToken(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns("a-token");

        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Staging);

        var services = new ServiceCollection();
        services.AddSingleton(tokenProvider);
        services
            .AddAltinn(
                environment,
                new MaskinportenConfiguration
                {
                    Scopes = ["shared:scope"],
                    PrivateKey = "some-private-key",
                    CertificateChain = "some-certificate-chain",
                    IntegrationId = "some-integration-id",
                }
            )
            .AddEvents();

        services
            .AddHttpClient(DependencyInjectionExtensions.AltinnEventsApiClientKey)
            .ConfigurePrimaryHttpMessageHandler(() =>
                new StubHandler(HttpStatusCode.BadRequest, ProblemBody, "application/problem+json")
            );

        using var scope = services.BuildServiceProvider().CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IAltinnEventsClient>();

        var exception = await Should.ThrowAsync<ApiException>(() =>
            client.Subscribe(new AltinnSubscriptionRequest { TypeFilter = "x" })
        );

        exception.Message.ShouldContain("Altinn Events request POST");
        exception.Message.ShouldContain("The subscription endpoint is not reachable.");
        exception.ResponseStatusCode.ShouldBe(400);
    }

    [Fact]
    public async Task DeclaredProblem_NumericErrorCodeAndCode_AreReadFromTheBody()
    {
        // Altinn Correspondence sends errorCode as a number and a code its declared
        // ProblemDetails type lacks; the generated client keeps neither.
        var exception = await Should.ThrowAsync<ApiException>(() =>
            GetCorrespondence(
                HttpStatusCode.NotFound,
                """
                {
                  "title": "Not Found",
                  "status": 404,
                  "detail": "The requested correspondence was not found",
                  "code": "CORR-01001",
                  "errorCode": 1001
                }
                """
            )
        );

        var problem = exception.GetAltinnProblemDetails().ShouldNotBeNull();

        problem.Status.ShouldBe(404);
        problem.Detail.ShouldBe("The requested correspondence was not found");
        problem.Code.ShouldBe("CORR-01001");
        problem.ErrorCode.ShouldBe("1001");
    }

    // GET /correspondence/{id} declares no 409, so the generated client has no problem type for
    // it and only the captured body can supply the details.
    [Fact]
    public async Task UndeclaredStatus_ProblemBody_IsReturnedAsProblemDetails()
    {
        var exception = await Should.ThrowAsync<ApiException>(() =>
            GetCorrespondence(
                HttpStatusCode.Conflict,
                """{"title":"Conflict","status":409,"detail":"Already exists","errorCode":1034}"""
            )
        );

        var problem = exception.GetAltinnProblemDetails().ShouldNotBeNull();

        problem.Status.ShouldBe(409);
        problem.Detail.ShouldBe("Already exists");
        problem.ErrorCode.ShouldBe("1034");
    }

    [Theory]
    [InlineData("""{"traceId":"00-abc-01"}""")]
    [InlineData("""{"instance":"/correspondence/1"}""")]
    [InlineData("""{"statusDescription":"Conflict"}""")]
    [InlineData("""{"validationErrors":[{"code":"X","detail":"Bad","paths":["/a"]}]}""")]
    [InlineData("""{"errors":{"Id":["Invalid"]}}""")]
    public async Task UndeclaredStatus_ProblemBodyWithAnySupportedMember_IsReturned(string body)
    {
        var exception = await Should.ThrowAsync<ApiException>(() =>
            GetCorrespondence(HttpStatusCode.Conflict, body)
        );

        exception.GetAltinnProblemDetails().ShouldNotBeNull().ShouldNotBe(new());
    }

    [Theory]
    [InlineData("upstream exploded", "text/plain")]
    [InlineData("""{"message":"not a problem document"}""", "application/json")]
    [InlineData("""{"status":"failed"}""", "application/json")]
    [InlineData("""{"title":5,"errorCode":true,"errors":[]}""", "application/json")]
    [InlineData("""{"validationErrors":[],"errors":{}}""", "application/json")]
    [InlineData("""["title"]""", "application/json")]
    [InlineData("""{"title":"Conflict","detail":""", "application/problem+json")]
    public async Task UndeclaredStatus_WithoutAProblemBody_HasNoProblemDetails(
        string body,
        string mediaType
    )
    {
        var exception = await Should.ThrowAsync<ApiException>(() =>
            GetCorrespondence(HttpStatusCode.Conflict, body, mediaType)
        );

        exception.GetAltinnProblemDetails().ShouldBeNull();
        exception.ResponseStatusCode.ShouldBe(409);
    }

    [Fact]
    public async Task TruncatedProblemBody_KeepsTheGeneratedProblemDetails()
    {
        var detail = new string('x', AltinnErrorResponseCaptureHandler.MaxBodyLength);
        var exception = await Should.ThrowAsync<ApiException>(() =>
            GetCorrespondence(
                HttpStatusCode.NotFound,
                $$"""{"status":404,"detail":"{{detail}}","errorCode":1001}"""
            )
        );

        var problem = exception.GetAltinnProblemDetails().ShouldNotBeNull();

        problem.Detail.ShouldBe(detail);
        problem.ErrorCode.ShouldBeNull();
    }

    private static async Task GetCorrespondence(
        HttpStatusCode status,
        string body,
        string mediaType = "application/problem+json"
    )
    {
        using var scope = RegisteredServices(
                services => services.AddCorrespondence(),
                DependencyInjectionExtensions.AltinnCorrespondenceApiClientKey,
                new StubHandler(status, body, mediaType)
            )
            .CreateScope();
        await scope
            .ServiceProvider.GetRequiredService<IAltinnCorrespondenceClient>()
            .GetCorrespondence(Guid.NewGuid());
    }

    private static ServiceProvider RegisteredServices(
        Func<IAltinnBuilder, IAltinnBuilder> register,
        string clientKey,
        HttpMessageHandler handler
    )
    {
        var tokenProvider = Substitute.For<IAltinnTokenProvider>();
        tokenProvider
            .GetToken(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns("a-token");

        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Staging);

        var services = new ServiceCollection();
        services.AddSingleton(tokenProvider);
        register(
            services.AddAltinn(
                environment,
                new MaskinportenConfiguration
                {
                    Scopes = ["shared:scope"],
                    PrivateKey = "some-private-key",
                    CertificateChain = "some-certificate-chain",
                    IntegrationId = "some-integration-id",
                }
            )
        );
        services.AddHttpClient(clientKey).ConfigurePrimaryHttpMessageHandler(() => handler);
        return services.BuildServiceProvider();
    }

    private sealed class CountingReadStream(byte[] data) : MemoryStream(data)
    {
        public long BytesRead { get; private set; }

        public override int Read(Span<byte> buffer)
        {
            var bytesRead = base.Read(buffer);
            BytesRead += bytesRead;
            return bytesRead;
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default
        )
        {
            var bytesRead = await base.ReadAsync(buffer, cancellationToken);
            BytesRead += bytesRead;
            return bytesRead;
        }
    }
}
