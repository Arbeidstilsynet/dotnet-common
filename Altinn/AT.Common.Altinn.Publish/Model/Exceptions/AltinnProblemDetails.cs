using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Altinn.Model.Exceptions;

/// <summary>
/// The problem details document an Altinn API returned with an error response, as defined by
/// RFC 9457, extended with the Altinn-specific fields the various APIs add.
/// </summary>
/// <remarks>
/// Obtained from a caught <see cref="Microsoft.Kiota.Abstractions.ApiException"/> via
/// <c>GetAltinnProblemDetails()</c>. Every property is optional: the APIs populate different
/// subsets, and an error response need not carry a problem details document at all.
/// </remarks>
public record AltinnProblemDetails
{
    /// <summary>
    /// A URI identifying the problem type.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    /// A short, human-readable summary of the problem type.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>
    /// The HTTP status code for this occurrence of the problem.
    /// </summary>
    [JsonPropertyName("status")]
    public int? Status { get; init; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    /// <summary>
    /// A URI identifying this specific occurrence of the problem.
    /// </summary>
    [JsonPropertyName("instance")]
    public string? Instance { get; init; }

    /// <summary>
    /// An Altinn-specific problem code. Populated by the correspondence and Dialogporten APIs.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    /// <summary>
    /// An Altinn-specific error code. Populated by the correspondence API.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; init; }

    /// <summary>
    /// A description of the status. Populated by the correspondence and Dialogporten APIs.
    /// </summary>
    [JsonPropertyName("statusDescription")]
    public string? StatusDescription { get; init; }

    /// <summary>
    /// Per-field validation failures. Populated by the Dialogporten and apps APIs.
    /// </summary>
    [JsonPropertyName("validationErrors")]
    public List<AltinnValidationError>? ValidationErrors { get; init; }

    /// <summary>
    /// Validation failures keyed by the member they apply to, as produced by ASP.NET Core model
    /// validation.
    /// </summary>
    [JsonPropertyName("errors")]
    public Dictionary<string, List<string>>? Errors { get; init; }

    /// <summary>
    /// The trace identifier for the failing request, useful when reporting a problem to Altinn.
    /// </summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; init; }
}

/// <summary>
/// A single validation failure within an <see cref="AltinnProblemDetails"/>.
/// </summary>
public record AltinnValidationError
{
    /// <summary>
    /// A code identifying the validation rule that failed.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    /// <summary>
    /// A human-readable explanation of the failure.
    /// </summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    /// <summary>
    /// The paths within the request that the failure applies to.
    /// </summary>
    [JsonPropertyName("paths")]
    public List<string>? Paths { get; init; }
}
