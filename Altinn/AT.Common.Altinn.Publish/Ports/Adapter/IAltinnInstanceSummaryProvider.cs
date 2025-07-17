using Altinn.App.Core.Models;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;

namespace Arbeidstilsynet.Common.Altinn.Ports.Adapter;

public interface IAltinnInstanceSummaryProvider
{
    public Task<AltinnInstanceSummary> GetSummary(
        CloudEvent cloudEvent,
        string? mainDocumentDataTypeName = "skjema"
    );
}
