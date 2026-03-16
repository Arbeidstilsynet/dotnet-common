using System.Net;

namespace Arbeidstilsynet.Common.Altinn.Model.Exceptions;

public class AltinnHttpRequestException(
    HttpRequestMessage? request,
    HttpStatusCode statusCode,
    string? responseBody,
    AltinnProblemDetails? problemDetails,
    string message
) : HttpRequestException(message, inner: null, statusCode)
{
    public string? ResponseBody { get; } = responseBody;
    public HttpRequestMessage? Request { get; } = request;
    public AltinnProblemDetails? ProblemDetails { get; } = problemDetails;
}
