namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

/// <summary>
/// The role the authenticated party has for the correspondences to look up.
/// </summary>
public enum CorrespondencesRoleType
{
    /// <summary>
    /// Look up correspondences where the party is the recipient.
    /// </summary>
    Recipient,

    /// <summary>
    /// Look up correspondences where the party is the sender.
    /// </summary>
    Sender,

    /// <summary>
    /// Look up correspondences where the party is either the recipient or the sender.
    /// </summary>
    RecipientAndSender,
}
