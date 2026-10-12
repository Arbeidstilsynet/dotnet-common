using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Model.Exceptions;

/// <summary>
/// The base exception for failures while creating an Altinn instance summary.
/// </summary>
/// <param name="message">The message that describes the error.</param>
/// <param name="instanceId">The identifier of the affected instance.</param>
/// <param name="appId">The identifier of the affected application.</param>
/// <param name="innerException">The exception that caused the failure.</param>
public abstract class AltinnInstanceSummaryException(
    string message,
    string? instanceId,
    string? appId,
    Exception? innerException = null
) : Exception(message, innerException)
{
    /// <summary>
    /// Gets the identifier of the affected instance.
    /// </summary>
    public string? InstanceId { get; } = instanceId;

    /// <summary>
    /// Gets the identifier of the affected application.
    /// </summary>
    public string? AppId { get; } = appId;
}

/// <summary>
/// The exception thrown when an instance does not contain its expected main data element.
/// </summary>
/// <param name="instance">The instance missing its main data element.</param>
/// <param name="expectedMainDataType">The expected main data type.</param>
/// <param name="existingDataTypes">The data types found in the instance.</param>
public sealed class AltinnMainDataElementNotFoundException(
    AltinnInstance instance,
    string expectedMainDataType,
    IEnumerable<string?> existingDataTypes
)
    : AltinnInstanceSummaryException(
        $"Main document with data type '{expectedMainDataType}' was not found in AltinnInstance '{instance.Id}' from app '{instance.AppId}'. Existing data types: [{string.Join(", ", existingDataTypes)}]",
        instance.Id,
        instance.AppId
    )
{
    /// <summary>
    /// Gets the expected main data type.
    /// </summary>
    public string ExpectedMainDataType { get; } = expectedMainDataType;

    /// <summary>
    /// Gets the data types found in the instance.
    /// </summary>
    public IReadOnlyCollection<string?> ExistingDataTypes { get; } = [.. existingDataTypes];
}

/// <summary>
/// The exception thrown when an Altinn instance has no owner party identifier.
/// </summary>
/// <param name="instance">The instance missing its owner party identifier.</param>
public sealed class AltinnInstanceOwnerPartyIdMissingException(AltinnInstance instance)
    : AltinnInstanceSummaryException(
        $"AltinnInstance owner party id is required for AltinnInstance '{instance.Id}' from app '{instance.AppId}'.",
        instance.Id,
        instance.AppId
    );

/// <summary>
/// The exception thrown when an Altinn data element has no identifier.
/// </summary>
/// <param name="instance">The instance containing the data element.</param>
/// <param name="dataElement">The data element missing its identifier.</param>
public sealed class AltinnDataElementIdMissingException(
    AltinnInstance instance,
    DataElement dataElement
)
    : AltinnInstanceSummaryException(
        $"Data element id is required for data type '{dataElement.DataType}' in AltinnInstance '{instance.Id}' from app '{instance.AppId}'.",
        instance.Id,
        instance.AppId
    )
{
    /// <summary>
    /// Gets the data type of the affected data element.
    /// </summary>
    public string? DataType { get; } = dataElement.DataType;
}
