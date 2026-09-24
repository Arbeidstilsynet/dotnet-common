namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

/// <summary>
/// Identifies a data element belonging to an Altinn instance.
/// </summary>
public record InstanceDataRequest
{
    /// <summary>
    /// Gets the instance address.
    /// </summary>
    public required InstanceRequest InstanceRequest { get; init; }

    /// <summary>
    /// Gets the data element identifier.
    /// </summary>
    public required Guid DataId { get; init; }
}
