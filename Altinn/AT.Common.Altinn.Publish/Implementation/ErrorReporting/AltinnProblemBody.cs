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
    private static readonly string[] ProblemMembers =
    [
        "type",
        "title",
        "status",
        "detail",
        "code",
        "errorCode",
    ];

    /// <summary>
    /// The problem details in <paramref name="body"/>, or <see langword="null"/> if it is not a
    /// complete JSON problem details document (for example a truncated or non-JSON body).
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

            if (
                root.ValueKind != JsonValueKind.Object
                || !ProblemMembers.Any(member => root.TryGetProperty(member, out _))
            )
            {
                return null;
            }

            return new AltinnProblemDetails
            {
                Type = Text(root, "type"),
                Title = Text(root, "title"),
                Status =
                    root.TryGetProperty("status", out var status)
                    && status.ValueKind == JsonValueKind.Number
                    && status.TryGetInt32(out var value)
                        ? value
                        : null,
                Detail = Text(root, "detail"),
                Instance = Text(root, "instance"),
                Code = Text(root, "code"),
                ErrorCode = Text(root, "errorCode"),
                StatusDescription = Text(root, "statusDescription"),
                TraceId = Text(root, "traceId"),
                ValidationErrors = Member<List<AltinnValidationError>>(root, "validationErrors"),
                Errors = Member<Dictionary<string, List<string>>>(root, "errors"),
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// A string member as is, or a number as written, so that a numeric code survives.
    /// </summary>
    private static string? Text(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var member))
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

    private static T? Member<T>(JsonElement root, string name)
        where T : class
    {
        if (!root.TryGetProperty(name, out var member) || member.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        try
        {
            return member.Deserialize<T>();
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
