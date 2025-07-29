namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

public record InstanceRequest
{
    public required string InstanceOwnerPartyId { get; init; }
    public required Guid InstanceGuid { get; init; }
}
