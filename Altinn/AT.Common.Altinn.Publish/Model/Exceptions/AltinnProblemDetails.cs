using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Altinn.Model.Exceptions;

/// <summary>
/// Represents the ProblemDetails response returned by Altinn APIs,
/// extended with Altinn-specific fields such as <see cref="Code"/> and <see cref="ValidationErrors"/>.
/// </summary>
public sealed class AltinnProblemDetails
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("validationErrors")]
    public List<AltinnValidationError>? ValidationErrors { get; set; }

    [JsonPropertyName("errors")]
    public Dictionary<string, List<string>>? Errors { get; set; }

    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }
}

/// <summary>
/// Represents a single validation error returned by Altinn APIs.
/// </summary>
public sealed class AltinnValidationError
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("paths")]
    public List<string>? Paths { get; set; }
}
