using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;

namespace Arbeidstilsynet.Common.Altinn.Model.Adapter
{
    /// <summary>
    /// A possible altinn receiver
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(Organization), "organization")]
    [JsonDerivedType(typeof(NorwegianCitizen), "norwegianCitizen")]
    [JsonDerivedType(typeof(SelfRegisteredUser), "selfRegisteredUser")]
    public interface IAltinnRecipient
    {
        /// <summary>
        /// Creates a valid altinn URN string for the receiver type
        /// </summary>
        /// <returns>A valid altinn URN</returns>
        public string ToAltinnRessourceFormat();
    }

    /// <summary>
    /// Represents an organization
    /// </summary>
    public record Organization : IAltinnRecipient
    {
        /// <summary>
        /// A valid organization number
        /// </summary>
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Invalid organization number format.")]
        public required string OrgNumber { get; init; }

        /// <summary>
        /// Creates a valid altinn URN string for the organization receiver type
        /// </summary>
        public string ToAltinnRessourceFormat()
        {
            return $"urn:altinn:organization:identifier-no:{OrgNumber}";
        }
    }

    /// <summary>
    /// Represents an norwegian citizen
    /// </summary>
    public record NorwegianCitizen : IAltinnRecipient
    {
        /// <summary>
        /// SSN
        /// </summary>
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Invalid SSN format.")]
        public required string SosialSecurityNumber { get; init; }

        /// <summary>
        /// Creates a valid altinn URN string for the ssn receiver type
        /// </summary>
        public string ToAltinnRessourceFormat()
        {
            return $"urn:altinn:person:identifier-no:{SosialSecurityNumber}";
        }
    }

    /// <summary>
    /// Represents a default user registered by e-mail
    /// </summary>
    public record SelfRegisteredUser : IAltinnRecipient
    {
        /// <summary>
        /// A valid e-mail address
        /// </summary>
        [EmailAddress]
        public required string EmailAddress { get; init; }

        /// <summary>
        /// Creates a valid altinn URN string for the email receiver type
        /// </summary>
        public string ToAltinnRessourceFormat()
        {
            return $"urn:altinn:person:idporten-email:{EmailAddress}";
        }
    }
}
