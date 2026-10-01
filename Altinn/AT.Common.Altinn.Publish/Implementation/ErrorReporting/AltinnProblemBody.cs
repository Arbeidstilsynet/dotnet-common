using System.Text.Json;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.ErrorReporting;

/// <summary>
/// Reads a captured error response body as a problem details document, tolerating what the
/// generated clients cannot: a numeric <c>errorCode</c>, and a document for a status code the
/// specification does not describe.
/// </summary>
internal static class AltinnProblemBody
{
    /// <summary>
    /// The problem details in <paramref name="body"/>, or <see langword="null"/> if it is not a
    /// complete JSON object with at least one supported problem details member of the expected
    /// type (for example a truncated, non-JSON or unrelated body).
    /// </summary>
    public static AltinnProblemDetails? Parse(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var problem = new AltinnProblemDetails
            {
                Type = Text(root, "type"),
                Title = Text(root, "title"),
                Status = Status(root),
                Detail = Text(root, "detail"),
                Instance = Text(root, "instance"),
                Code = Text(root, "code"),
                ErrorCode = ErrorCode(root),
                StatusDescription = Text(root, "statusDescription"),
                TraceId = Text(root, "traceId"),
                ValidationErrors = Member<List<AltinnValidationError>>(
                    root,
                    "validationErrors",
                    JsonValueKind.Array
                ),
                Errors = Member<Dictionary<string, List<string>>>(
                    root,
                    "errors",
                    JsonValueKind.Object
                ),
            };

            return problem == new AltinnProblemDetails() ? null : problem;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? Text(JsonElement root, string name) =>
        root.TryGetProperty(name, out var member) && member.ValueKind == JsonValueKind.String
            ? member.GetString()
            : null;

    private static int? Status(JsonElement root) =>
        root.TryGetProperty("status", out var member)
        && member.ValueKind == JsonValueKind.Number
        && member.TryGetInt32(out var status)
            ? status
            : null;

    /// <summary>
    /// Altinn Correspondence sends <c>errorCode</c> as a number although its specification
    /// declares a string; a number is kept as written.
    /// </summary>
    private static string? ErrorCode(JsonElement root)
    {
        if (!root.TryGetProperty("errorCode", out var member))
        {
            return null;
        }

        return member.ValueKind switch
        {
            JsonValueKind.String => member.GetString(),
            JsonValueKind.Number => member.GetRawText(),
            _ => null,
        };
    }

    private static T? Member<T>(JsonElement root, string name, JsonValueKind kind)
        where T : class
    {
        if (!root.TryGetProperty(name, out var member) || member.ValueKind != kind)
        {
            return null;
        }

        try
        {
            // An empty collection carries no problem information on its own.
            return
                member.Deserialize<T>()
                    is var value
                        and not System.Collections.ICollection { Count: 0 }
                ? value
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
