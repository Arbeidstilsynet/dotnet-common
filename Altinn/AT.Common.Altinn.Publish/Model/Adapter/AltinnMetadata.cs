namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

public record AltinnMetadata
{
    public required Guid InstanceGuid { get; init; }
    public required string InstanceOwnerPartyId { get; init; }
    public required string Org { get; init; }
    public required string App { get; init; }
    public required Dictionary<string, string> DataValues { get; init; }
    public string? OrganisationNumber { get; init; }

    public DateTime? ProcessStarted { get; init; }

    public DateTime? ProcessEnded { get; init; }

    public string? DialogId { get; init; }
}
