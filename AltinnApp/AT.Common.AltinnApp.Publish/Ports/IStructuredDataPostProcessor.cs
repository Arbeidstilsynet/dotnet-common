using Arbeidstilsynet.Common.AltinnApp.Model;

namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Post-processing hook for the structured data feature. It runs at process end, after the structured data
/// has been verified. The package does not delete the App data model.
/// </summary>
/// <remarks>
/// Unlike a plain <see cref="Altinn.App.Core.Features.IProcessEnd"/>, a post-processor is guaranteed that both
/// the structured data element and the main PDF element exist on the instance.
/// <br/>
/// Post-processors should be idempotent: if a later step fails, process end may be retried and they will run again.
/// <br/>
/// Altinn-managed autoDeleteOnProcessEnd runs before these post-processors in Altinn 8.12.11.
/// When enabled, do not depend on the form model being available, including during recovery.
/// Automatic retry of a failed post-processor is not guaranteed.
/// <br/>
/// Post-processors are responsible for persisting their own changes, e.g. by calling
/// <see cref="Altinn.App.Core.Internal.Data.IDataClient.Update(Altinn.Platform.Storage.Interface.Models.Instance, Altinn.Platform.Storage.Interface.Models.DataElement)"/>
/// after modifying a <see cref="Altinn.Platform.Storage.Interface.Models.DataElement"/>.
/// <br/>
/// Register implementations with
/// <see cref="DependencyInjection.DependencyInjectionExtensions.AddStructuredDataPostProcessor{T}"/>.
/// </remarks>
public interface IStructuredDataPostProcessor
{
    /// <summary>
    /// Post-processes the instance and its structured data and main PDF data elements.
    /// </summary>
    /// <param name="context">The instance and the resolved data elements.</param>
    Task Process(StructuredDataPostProcessingContext context);
}
