using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Ports.Adapter;

/// <summary>
/// Provides convenience operations for reading Altinn instances and their data elements.
/// </summary>
public interface IAltinnStorageAdapter
{
    /// <summary>
    /// Gets an instance by its identifier.
    /// </summary>
    Task<AltinnInstance?> GetInstance(
        Guid instanceId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the data elements belonging to an instance.
    /// </summary>
    Task<IEnumerable<DataElement>?> GetDataElements(
        Guid instanceId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a data element by its identifier.
    /// </summary>
    Task<DataElement?> GetDataElement(
        Guid instanceId,
        Guid dataElementId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the content of a data element.
    /// </summary>
    Task<Stream?> GetDataElementContent(
        Guid instanceId,
        Guid dataElementId,
        CancellationToken cancellationToken = default
    );
}
