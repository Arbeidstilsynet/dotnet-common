using Arbeidstilsynet.Common.Altinn.DependencyInjection;

namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

/// <summary>
/// Sets up a subscription for Altinn events for a specific Altinn application.
/// </summary>
public record SubscriptionRequestDto
{
    /// <summary>
    /// The callback URL where Altinn will send event notifications.
    /// <br/>
    /// How to implement it is documented here https://docs.altinn.studio/en/events/subscribe-to-events/developer-guides/setup-subscription/#endpoint-1
    /// </summary>
    public required Uri CallbackUrl { get; init; }

    /// <summary>
    /// The Altinn application identifier for which to set up the subscription.
    /// </summary>
    /// <remarks>Any org prefix will be removed. Specify the orgId in <see cref="AltinnConfiguration.OrgId"/></remarks>
    public required string AltinnAppId { get; init; }
}
