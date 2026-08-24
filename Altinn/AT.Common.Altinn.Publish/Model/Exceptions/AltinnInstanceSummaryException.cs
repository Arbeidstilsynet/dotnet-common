using Arbeidstilsynet.Common.Altinn.Storage.Models;

namespace Arbeidstilsynet.Common.Altinn.Model.Exceptions;

public abstract class AltinnInstanceSummaryException(
    string message,
    string? instanceId,
    string? appId,
    Exception? innerException = null
) : Exception(message, innerException)
{
    public string? InstanceId { get; } = instanceId;
    public string? AppId { get; } = appId;
}

public sealed class AltinnMainDataElementNotFoundException(
    Instance instance,
    string expectedMainDataType,
    IEnumerable<string?> existingDataTypes
)
    : AltinnInstanceSummaryException(
        $"Main document with data type '{expectedMainDataType}' was not found in instance '{instance.Id}' from app '{instance.AppId}'. Existing data types: [{string.Join(", ", existingDataTypes)}]",
        instance.Id,
        instance.AppId
    )
{
    public string ExpectedMainDataType { get; } = expectedMainDataType;
    public IReadOnlyCollection<string?> ExistingDataTypes { get; } = [.. existingDataTypes];
}

public sealed class AltinnInstanceOwnerPartyIdMissingException(Instance instance)
    : AltinnInstanceSummaryException(
        $"Instance owner party id is required for instance '{instance.Id}' from app '{instance.AppId}'.",
        instance.Id,
        instance.AppId
    );

public sealed class AltinnDataElementIdMissingException(
    Instance instance,
    DataElement dataElement
)
    : AltinnInstanceSummaryException(
        $"Data element id is required for data type '{dataElement.DataType}' in instance '{instance.Id}' from app '{instance.AppId}'.",
        instance.Id,
        instance.AppId
    )
{
    public string? DataType { get; } = dataElement.DataType;
}

