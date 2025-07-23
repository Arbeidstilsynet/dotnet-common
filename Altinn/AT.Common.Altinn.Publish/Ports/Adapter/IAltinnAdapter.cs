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
        Action<AltinnAppConfiguration>? appConfigAction = null
    );

    public Task<Subscription> SubscribeForCompletedProcessEvents(
        SubscriptionRequestDto subscriptionRequestDto
    );

    public Task<AltinnInstanceSummary[]> GetNonCompletedInstances(
        string appId,
        bool? ProcessIsComplete = true,
        string? ExcludeConfirmedBy = DependencyInjectionExtensions.AltinnOrgIdentifier,
        Action<AltinnAppConfiguration>? appConfigAction = null
    );
}
