using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.Altinn.Model.Api.Response;

/// <summary>
/// Represents the result of a dialog lookup by instance reference in end user context.
/// </summary>
public class DialogportenLookupResponse
{
    /// <summary>
    /// The unique identifier for the resolved dialog.
    /// </summary>
    [JsonPropertyName("dialogId")]
    public Guid DialogId { get; set; }

    /// <summary>
    /// The instance reference that was used for the lookup.
    /// </summary>
    [JsonPropertyName("instanceRef")]
    public string InstanceRef { get; set; } = string.Empty;

    /// <summary>
    /// The party code representing the organization or person that the dialog belongs to.
    /// </summary>
    [JsonPropertyName("party")]
    public string Party { get; set; } = string.Empty;

    /// <summary>
    /// Information about the service resource associated with the dialog.
    /// </summary>
    [JsonPropertyName("serviceResource")]
    public DialogportenServiceResource? ServiceResource { get; set; }

    /// <summary>
    /// Information about the service owner of the dialog.
    /// </summary>
    [JsonPropertyName("serviceOwner")]
    public DialogportenServiceOwner? ServiceOwner { get; set; }

    /// <summary>
    /// The localized title of the dialog.
    /// </summary>
    [JsonPropertyName("title")]
    public List<DialogportenLocalization>? Title { get; set; }

    /// <summary>
    /// Authorization evidence for the end user's access to the dialog.
    /// </summary>
    [JsonPropertyName("authorizationEvidence")]
    public DialogportenAuthorizationEvidence? AuthorizationEvidence { get; set; }
}

/// <summary>
/// Represents authorization evidence for a dialog lookup.
/// </summary>
public class DialogportenAuthorizationEvidence
{
    /// <summary>
    /// The current authentication level of the user.
    /// </summary>
    [JsonPropertyName("currentAuthenticationLevel")]
    public int CurrentAuthenticationLevel { get; set; }

    /// <summary>
    /// Whether access is granted via a role.
    /// </summary>
    [JsonPropertyName("viaRole")]
    public bool ViaRole { get; set; }

    /// <summary>
    /// Whether access is granted via an access package.
    /// </summary>
    [JsonPropertyName("viaAccessPackage")]
    public bool ViaAccessPackage { get; set; }

    /// <summary>
    /// Whether access is granted via resource delegation.
    /// </summary>
    [JsonPropertyName("viaResourceDelegation")]
    public bool ViaResourceDelegation { get; set; }

    /// <summary>
    /// Whether access is granted via instance delegation.
    /// </summary>
    [JsonPropertyName("viaInstanceDelegation")]
    public bool ViaInstanceDelegation { get; set; }
}

/// <summary>
/// Represents information about a service resource in Dialogporten.
/// </summary>
public class DialogportenServiceResource
{
    /// <summary>
    /// The service resource identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Whether the service resource is delegable.
    /// </summary>
    [JsonPropertyName("isDelegable")]
    public bool IsDelegable { get; set; }

    /// <summary>
    /// The minimum authentication level required for the service resource.
    /// </summary>
    [JsonPropertyName("minimumAuthenticationLevel")]
    public int MinimumAuthenticationLevel { get; set; }

    /// <summary>
    /// The localized name of the service resource.
    /// </summary>
    [JsonPropertyName("name")]
    public List<DialogportenLocalization>? Name { get; set; }
}

/// <summary>
/// Represents information about a service owner in Dialogporten.
/// </summary>
public class DialogportenServiceOwner
{
    /// <summary>
    /// The organization number of the service owner.
    /// </summary>
    [JsonPropertyName("orgNumber")]
    public string OrgNumber { get; set; } = string.Empty;

    /// <summary>
    /// The service owner code.
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The localized name of the service owner.
    /// </summary>
    [JsonPropertyName("name")]
    public List<DialogportenLocalization>? Name { get; set; }
}

/// <summary>
/// Represents a localized text value in Dialogporten.
/// </summary>
public class DialogportenLocalization
{
    /// <summary>
    /// The localized text value.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// The language code of the localization in ISO 639-1 format.
    /// </summary>
    [JsonPropertyName("languageCode")]
    public string LanguageCode { get; set; } = string.Empty;
}
