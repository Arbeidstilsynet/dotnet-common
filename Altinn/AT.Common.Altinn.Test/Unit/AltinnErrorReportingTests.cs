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
}
