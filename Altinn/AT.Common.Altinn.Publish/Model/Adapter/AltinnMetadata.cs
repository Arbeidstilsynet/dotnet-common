namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

/// <summary>
/// Contains metadata for an Altinn instance.
/// </summary>
public record AltinnMetadata
{
    /// <summary>
    /// Gets the instance identifier.
    /// </summary>
    public required Guid InstanceGuid { get; init; }

    /// <summary>
    /// Gets the party identifier of the instance owner.
    /// </summary>
    public required string InstanceOwnerPartyId { get; init; }

    /// <summary>
    /// Gets the identifier of the organization that owns the application.
    /// </summary>
    public required string Org { get; init; }

    /// <summary>
    /// Gets the application identifier.
    /// </summary>
    public required string App { get; init; }

    /// <summary>
    /// Gets the instance data values.
    /// </summary>
    public required Dictionary<string, string> DataValues { get; init; }

    /// <summary>
    /// Gets the organization number of the instance owner.
    /// </summary>
    public string? OrganisationNumber { get; init; }

    /// <summary>
    /// Gets the time when processing started.
    /// </summary>
    public DateTime? ProcessStarted { get; init; }

    /// <summary>
    /// Gets the time when processing ended.
    /// </summary>
    public DateTime? ProcessEnded { get; init; }

    /// <summary>
    /// Gets the associated dialog identifier.
    /// </summary>
    public string? DialogId { get; init; }
}
