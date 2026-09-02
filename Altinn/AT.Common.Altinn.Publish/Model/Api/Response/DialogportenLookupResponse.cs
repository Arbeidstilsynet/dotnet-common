using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Altinn.Model.Api.Response;

/// <summary>
/// Represents the result of a dialog lookup by instance reference in service owner context.
/// </summary>
public record DialogportenLookupResponse
{
    /// <summary>
    /// The unique identifier for the resolved dialog.
    /// </summary>
    [JsonPropertyName("dialogId")]
    public Guid? DialogId { get; init; }

    /// <summary>
    /// The instance reference that was used for the lookup.
    /// </summary>
    [JsonPropertyName("instanceRef")]
    public string? InstanceRef { get; init; }

    /// <summary>
    /// The party code representing the organization or person that the dialog belongs to.
    /// </summary>
    [JsonPropertyName("party")]
    public string? Party { get; init; }

    /// <summary>
    /// Information about the service resource associated with the dialog.
    /// </summary>
    [JsonPropertyName("serviceResource")]
    public DialogportenServiceResource? ServiceResource { get; init; }

    /// <summary>
    /// Information about the service owner of the dialog.
    /// </summary>
    [JsonPropertyName("serviceOwner")]
    public DialogportenServiceOwner? ServiceOwner { get; init; }

    /// <summary>
    /// The localized title of the dialog.
    /// </summary>
    [JsonPropertyName("title")]
    public List<DialogportenLocalization>? Title { get; init; }

    /// <summary>
    /// An optional non-sensitive title of the dialog.
    /// </summary>
    [JsonPropertyName("nonSensitiveTitle")]
    public List<DialogportenLocalization>? NonSensitiveTitle { get; init; }
}

/// <summary>
/// Represents information about a service resource in Dialogporten.
/// </summary>
public record DialogportenServiceResource
{
    /// <summary>
    /// The service resource identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Whether the service resource is delegable.
    /// </summary>
    [JsonPropertyName("isDelegable")]
    public bool? IsDelegable { get; init; }

    /// <summary>
    /// The minimum authentication level required for the service resource.
    /// </summary>
    [JsonPropertyName("minimumAuthenticationLevel")]
    public int? MinimumAuthenticationLevel { get; init; }

    /// <summary>
    /// The localized name of the service resource.
    /// </summary>
    [JsonPropertyName("name")]
    public List<DialogportenLocalization>? Name { get; init; }
}

/// <summary>
/// Represents information about a service owner in Dialogporten.
/// </summary>
public record DialogportenServiceOwner
{
    /// <summary>
    /// The organization number of the service owner.
    /// </summary>
    [JsonPropertyName("orgNumber")]
    public string? OrgNumber { get; init; }

    /// <summary>
    /// The service owner code.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    /// <summary>
    /// The localized name of the service owner.
    /// </summary>
    [JsonPropertyName("name")]
    public List<DialogportenLocalization>? Name { get; init; }
}

/// <summary>
/// Represents a localized text value in Dialogporten.
/// </summary>
public record DialogportenLocalization
{
    /// <summary>
    /// The localized text value.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; init; }

    /// <summary>
    /// The language code of the localization in ISO 639-1 format.
    /// </summary>
    [JsonPropertyName("languageCode")]
    public string? LanguageCode { get; init; }
}
