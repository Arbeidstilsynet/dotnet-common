namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

public record AltinnMetadata
{
    public Guid? InstanceGuid { get; init; }
    public string? InstanceOwnerPartyId { get; init; }
    public string? Org { get; init; }
    public string? App { get; init; }

    public string? OrganisationNumber { get; init; }

    public DateTime? ProcessStarted { get; init; }

    public DateTime? ProcessEnded { get; init; }
}
