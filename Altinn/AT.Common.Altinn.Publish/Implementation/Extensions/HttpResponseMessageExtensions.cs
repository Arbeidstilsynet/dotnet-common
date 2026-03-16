using System.Text;
using System.Text.Json;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Extensions;

internal static class HttpResponseMessageExtensions
{
    public static async Task EnsureSuccessStatusCodeWithBody(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();
        var problemDetails = TryParseProblemDetails(body);

        throw new AltinnHttpRequestException(
            response.RequestMessage,
            response.StatusCode,
            body,
            problemDetails,
            BuildMessage(response, body, problemDetails)
        );
    }

    private static AltinnProblemDetails? TryParseProblemDetails(string? body)
    {
        if (string.IsNullOrEmpty(body))
            return null;

        try
        {
            return JsonSerializer.Deserialize<AltinnProblemDetails>(body);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string BuildMessage(
        HttpResponseMessage response,
        string? body,
        AltinnProblemDetails? problemDetails
    )
    {
        var sb = new StringBuilder();
        sb.Append(
            $"Request to Altinn API failed. HTTP {(int)response.StatusCode} {response.StatusCode}"
        );
        sb.Append(
            $" from {response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}."
        );

        if (problemDetails is not null)
        {
            if (problemDetails.Title is not null)
                sb.Append($" {problemDetails.Title}.");

            if (problemDetails.Detail is not null)
                sb.Append($" {problemDetails.Detail}");

            if (problemDetails.ValidationErrors is { Count: > 0 })
            {
                sb.Append(" Validation errors:");
                foreach (var error in problemDetails.ValidationErrors)
                {
                    sb.Append($" [{string.Join(", ", error.Paths ?? [])}] {error.Detail}");
                }
            }

            if (problemDetails.TraceId is not null)
                sb.Append($" TraceId: {problemDetails.TraceId}.");
        }
        else
        {
            sb.Append($" Response body: {body}");
        }

        return sb.ToString();
    }
}
