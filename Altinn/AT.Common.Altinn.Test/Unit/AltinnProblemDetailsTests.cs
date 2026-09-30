using System.Net;
using System.Text;
using Arbeidstilsynet.Common.Altinn.Correspondence;
using Arbeidstilsynet.Common.Altinn.Dialogporten;
using Arbeidstilsynet.Common.Altinn.Events;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Bundle;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

/// <summary>
/// Covers surfacing the problem details Altinn returns with an error response.
/// </summary>
/// <remarks>
/// These drive real generated clients over a stubbed transport rather than constructing the
/// exception directly, because the behaviour under test is Kiota's error mapping: the generated
/// clients parse the problem details and throw them as the exception itself.
/// </remarks>
public class AltinnProblemDetailsTests
{
    private sealed class ProblemHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new HttpResponseMessage(status)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/problem+json"),
                }
            );
    }

    private static DefaultRequestAdapter Adapter(
        string baseUrl,
        HttpStatusCode status,
        string body
    ) =>
        new(
            new AnonymousAuthenticationProvider(),
            httpClient: new HttpClient(new ProblemHandler(status, body))
        )
        {
            BaseUrl = baseUrl,
        };

    [Fact]
    public async Task EventsProblemDetails_AreSurfaced()
    {
        const string body = """
            {
              "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
              "title": "Bad Request",
              "status": 400,
              "detail": "The subscription endpoint is not reachable.",
              "instance": "/subscriptions"
            }
            """;

        var sut = new AltinnEventsClient(
            new EventsApiClient(
                Adapter(
                    "https://platform.tt02.altinn.no/events/api/v1",
                    HttpStatusCode.BadRequest,
                    body
                )
            )
        );

        var exception = await Should.ThrowAsync<ApiException>(() =>
            sut.Subscribe(new AltinnSubscriptionRequest { TypeFilter = "x" })
        );

        var problem = exception.GetAltinnProblemDetails();

        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("Bad Request");
        problem.Status.ShouldBe(400);
        problem.Detail.ShouldBe("The subscription endpoint is not reachable.");
        problem.Instance.ShouldBe("/subscriptions");
        problem.Type.ShouldNotBeNull();
    }

    [Fact]
    public async Task CorrespondenceProblemDetails_SurfaceTheAltinnSpecificFields()
    {
        const string body = """
            {
              "type": "about:blank",
              "title": "Not Found",
              "status": 404,
              "detail": "The correspondence does not exist.",
              "errorCode": "AC-1042",
              "traceId": "00-abc123-def456-01"
            }
            """;

        var sut = new AltinnCorrespondenceClient(
            new CorrespondenceApiClient(
                Adapter("https://platform.tt02.altinn.no", HttpStatusCode.NotFound, body)
            ),
            null!
        );

        var exception = await Should.ThrowAsync<ApiException>(() =>
            sut.GetCorrespondence(Guid.NewGuid())
        );

        var problem = exception.GetAltinnProblemDetails();

        problem.ShouldNotBeNull();
        problem.Status.ShouldBe(404);
        problem.Title.ShouldBe("Not Found");
        problem.Detail.ShouldBe("The correspondence does not exist.");
        problem.ErrorCode.ShouldBe("AC-1042");
        problem.TraceId.ShouldBe("00-abc123-def456-01");
    }

    [Fact]
    public async Task DialogportenProblemDetails_SurfaceValidationErrorsAndErrorDictionary()
    {
        const string body = """
            {
              "title": "Validation error",
              "status": 400,
              "code": "DIALOG_INVALID",
              "traceId": "trace-1",
              "validationErrors": [
                {
                  "code": "NOT_EMPTY",
                  "detail": "Party must not be empty.",
                  "paths": ["dialog.party", "dialog.serviceResource"]
                }
              ],
              "errors": {
                "dialog.party": ["Must not be empty.", "Must be a valid urn."],
                "dialog.id": ["Must be a UUIDv7."]
              }
            }
            """;

        var sut = new AltinnDialogportenClient(
            new DialogportenApiClient(
                Adapter(
                    "https://platform.tt02.altinn.no/dialogporten",
                    HttpStatusCode.BadRequest,
                    body
                )
            )
        );

        var exception = await Should.ThrowAsync<ApiException>(() =>
            sut.LookupDialog("some-instance-ref")
        );

        var problem = exception.GetAltinnProblemDetails();

        problem.ShouldNotBeNull();
        problem.Code.ShouldBe("DIALOG_INVALID");

        problem.ValidationErrors.ShouldNotBeNull();
        var validationError = problem.ValidationErrors.ShouldHaveSingleItem();
        validationError.Code.ShouldBe("NOT_EMPTY");
        validationError.Detail.ShouldBe("Party must not be empty.");
        validationError.Paths.ShouldBe(["dialog.party", "dialog.serviceResource"]);

        // The specification models `errors` as a free-form object, so the values arrive as Kiota's
        // untyped nodes and have to be unwrapped rather than cast.
        problem.Errors.ShouldNotBeNull();
        problem.Errors["dialog.party"].ShouldBe(["Must not be empty.", "Must be a valid urn."]);
        problem.Errors["dialog.id"].ShouldBe(["Must be a UUIDv7."]);
    }

    [Fact]
    public async Task StatusWithoutADeclaredProblemDetails_YieldsNull()
    {
        // The events specification declares problem details for 400/401/403 but not 500, so Kiota
        // throws a bare ApiException. Callers must get null rather than an exception.
        var sut = new AltinnEventsClient(
            new EventsApiClient(
                Adapter(
                    "https://platform.tt02.altinn.no/events/api/v1",
                    HttpStatusCode.InternalServerError,
                    "upstream exploded"
                )
            )
        );

        var exception = await Should.ThrowAsync<ApiException>(() =>
            sut.Subscribe(new AltinnSubscriptionRequest { TypeFilter = "x" })
        );

        exception.ResponseStatusCode.ShouldBe(500);
        exception.GetAltinnProblemDetails().ShouldBeNull();
    }

    [Fact]
    public void AnUnrelatedApiException_YieldsNull()
    {
        new ApiException("something else").GetAltinnProblemDetails().ShouldBeNull();
    }

    [Fact]
    public void ANullException_YieldsNull()
    {
        ApiException? exception = null;

        exception.GetAltinnProblemDetails().ShouldBeNull();
    }
}
