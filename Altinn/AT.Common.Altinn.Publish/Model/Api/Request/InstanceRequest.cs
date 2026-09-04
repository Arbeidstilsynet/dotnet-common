namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

/// <summary>
/// Identifies an Altinn instance.
/// </summary>
public record InstanceRequest
{
    /// <summary>
    /// Gets the party identifier of the instance owner.
    /// </summary>
    public required string InstanceOwnerPartyId { get; init; }

    /// <summary>
    /// Gets the instance identifier.
    /// </summary>
    public required Guid InstanceGuid { get; init; }
}
