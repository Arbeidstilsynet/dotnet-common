namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

public record InstanceDataRequest
{
    public required InstanceRequest InstanceRequest { get; init; }
    public required Guid DataId { get; init; }
}
