using Arbeidstilsynet.Common.AltinnApp.Model;

namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Post-processing hook for the structured data feature. It runs at process end, after the structured data
/// has been verified and before the App data model is deleted (unless kept by configuration).
/// </summary>
/// <remarks>
/// Unlike a plain <see cref="Altinn.App.Core.Features.IProcessEnd"/>, a post-processor is guaranteed that both
/// the structured data element and the main PDF element exist on the instance.
/// <br/>
/// Post-processors should be idempotent: if a later step fails, process end may be retried and they will run again.
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
