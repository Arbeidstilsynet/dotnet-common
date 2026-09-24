using Arbeidstilsynet.Common.Altinn.Model.Exceptions;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using AppsDataPostError = Arbeidstilsynet.Common.Altinn.Apps.Models.DataPostErrorResponse;
using AppsProblem = Arbeidstilsynet.Common.Altinn.Apps.Models.ProblemDetails;
using CorrespondenceAltinnProblem = Arbeidstilsynet.Common.Altinn.Correspondence.Models.AltinnProblemDetails;
using CorrespondenceProblem = Arbeidstilsynet.Common.Altinn.Correspondence.Models.ProblemDetails;
using DialogportenProblem = Arbeidstilsynet.Common.Altinn.Dialogporten.Models.ProblemDetails;
using DialogportenValidationError = Arbeidstilsynet.Common.Altinn.Dialogporten.Models.ProblemDetails_Error;
using EventsProblem = Arbeidstilsynet.Common.Altinn.Events.Models.ProblemDetails;
using StorageProblem = Arbeidstilsynet.Common.Altinn.Storage.Models.ProblemDetails;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

/// <summary>
/// Extensions for inspecting failures returned by the Altinn APIs.
/// </summary>
public static class AltinnExceptionExtensions
{
    /// <summary>
    /// Reads the problem details document Altinn returned with an error response.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The generated clients already parse the problem details and throw them as the exception
    /// itself, but the parsed types are internal to this package. This exposes their contents as a
    /// stable, serialisable model.
    /// </para>
    /// <para>
    /// Returns <see langword="null"/> when the response carried no problem details -- either
    /// because the endpoint does not declare one for that status code, or because the failure
    /// happened before a response was received.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     await storageClient.GetInstance(instanceGuid);
    /// }
    /// catch (ApiException e)
    /// {
    ///     var problem = e.GetAltinnProblemDetails();
    ///     logger.LogError("Altinn returned {Status}: {Title}", problem?.Status, problem?.Title);
    /// }
    /// </code>
    /// </example>
    public static AltinnProblemDetails? GetAltinnProblemDetails(this ApiException? exception)
    {
        return exception switch
        {
            StorageProblem problem => new AltinnProblemDetails
            {
                Type = problem.Type,
                Title = problem.Title,
                Status = problem.Status,
                Detail = problem.Detail,
                Instance = problem.Instance,
            },
            EventsProblem problem => new AltinnProblemDetails
            {
                Type = problem.Type,
                Title = problem.Title,
                Status = problem.Status,
                Detail = problem.Detail,
                Instance = problem.Instance,
            },
            AppsProblem problem => new AltinnProblemDetails
            {
                Type = problem.Type,
                Title = problem.Title,
                Status = problem.Status,
                Detail = problem.Detail,
                Instance = problem.Instance,
            },
            AppsDataPostError problem => new AltinnProblemDetails
            {
                Type = problem.Type,
                Title = problem.Title,
                Status = problem.Status,
                Detail = problem.Detail,
                Instance = problem.Instance,
                ValidationErrors =
                [
                    .. (problem.UploadValidationIssues ?? []).Select(
                        issue => new AltinnValidationError
                        {
                            Code = issue.Code,
                            Detail = issue.Description,
                            Paths = issue.Field is { } field ? [field] : null,
                        }
                    ),
                ],
            },
            CorrespondenceAltinnProblem problem => new AltinnProblemDetails
            {
                Type = problem.Type,
                Title = problem.Title,
                Status = problem.Status,
                Detail = problem.Detail,
                Instance = problem.Instance,
                Code = problem.Code,
                ErrorCode = problem.ErrorCode,
                StatusDescription = problem.StatusDescription,
                TraceId = problem.TraceId,
            },
            CorrespondenceProblem problem => new AltinnProblemDetails
            {
                Type = problem.Type,
                Title = problem.Title,
                Status = problem.Status,
                Detail = problem.Detail,
                Instance = problem.Instance,
                ErrorCode = problem.ErrorCode,
                TraceId = problem.TraceId,
            },
            DialogportenProblem problem => new AltinnProblemDetails
            {
                Type = problem.Type,
                Title = problem.Title,
                Status = problem.Status,
                Detail = problem.Detail,
                Instance = problem.Instance,
                Code = problem.Code,
                StatusDescription = problem.StatusDescription,
                TraceId = problem.TraceId,
                ValidationErrors = problem.ValidationErrors?.Select(ToValidationError).ToList(),
                Errors = ToErrorDictionary(problem.Errors?.AdditionalData),
            },
            _ => null,
        };
    }

    private static AltinnValidationError ToValidationError(DialogportenValidationError source) =>
        new()
        {
            Code = source.Code,
            Detail = source.Detail,
            Paths = source.Paths,
        };

    /// <summary>
    /// Converts the free-form <c>errors</c> member into the member-to-messages shape ASP.NET Core
    /// produces.
    /// </summary>
    /// <remarks>
    /// The specification models this as an untyped object, so the values arrive as Kiota's untyped
    /// nodes rather than strings and have to be unwrapped defensively.
    /// </remarks>
    private static Dictionary<string, List<string>>? ToErrorDictionary(
        IDictionary<string, object>? source
    )
    {
        if (source is null || source.Count == 0)
        {
            return null;
        }

        return source.ToDictionary(entry => entry.Key, entry => ToMessages(entry.Value));
    }

    private static List<string> ToMessages(object? value)
    {
        return value switch
        {
            null => [],
            string single => [single],
            UntypedString untyped => [untyped.GetValue() ?? string.Empty],
            UntypedArray array => [.. array.GetValue().SelectMany(ToMessages)],
            IEnumerable<object?> many => [.. many.SelectMany(ToMessages)],
            _ => [value.ToString() ?? string.Empty],
        };
    }
}
