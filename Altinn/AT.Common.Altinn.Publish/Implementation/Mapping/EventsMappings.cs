using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using GeneratedSubscription = Arbeidstilsynet.Common.Altinn.Events.Models.Subscription;
using GeneratedSubscriptionRequest = Arbeidstilsynet.Common.Altinn.Events.Models.SubscriptionRequestModel;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Mapping;

/// <summary>
/// Maps between the package's event models and the generated ones.
/// </summary>
internal static class EventsMappings
{
    public static AltinnSubscription ToAltinnSubscription(this GeneratedSubscription source)
    {
        return new AltinnSubscription
        {
            Id = source.Id ?? 0,
            EndPoint = ToUri(source.EndPoint),
            SourceFilter = ToUri(source.SourceFilter),
            TypeFilter = source.TypeFilter,
            Consumer = source.Consumer,
            CreatedBy = source.CreatedBy,
            Created = source.Created?.DateTime ?? default,
            Validated = source.Validated ?? false,
        };
    }

    public static GeneratedSubscriptionRequest ToGeneratedRequest(
        this AltinnSubscriptionRequest source
    )
    {
        return new GeneratedSubscriptionRequest
        {
            EndPoint = source.EndPoint?.ToString(),
            SourceFilter = source.SourceFilter?.ToString(),
            TypeFilter = source.TypeFilter,
        };
    }

    private static Uri? ToUri(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null;
}
