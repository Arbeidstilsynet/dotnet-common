using Altinn.App.Core.Infrastructure.Clients.Events;
using Altinn.App.Core.Models;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Microsoft.AspNetCore.Hosting;

namespace Arbeidstilsynet.Common.Altinn.Ports.Adapter;

public interface IAltinnAdapter
{
    public Task<AltinnInstanceSummary> GetSummary(
        CloudEvent cloudEvent,
        string? mainDocumentDataTypeName = "skjema"
    );

    public Task<Subscription> SubscribeForCompletedProcessEvents(
        SubscriptionRequestDto subscriptionRequestDto,
        IWebHostEnvironment webHostEnvironment
    );

    public Task<List<AltinnMetadata>> GetNonCompletedInstances(
        string appId,
        bool? ProcessIsComplete = true,
        string? ExcludeConfirmedBy = DependencyInjectionExtensions.AltinnOrgIdentifier
    );
}
