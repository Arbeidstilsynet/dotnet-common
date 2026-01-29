namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

public record SubscriptionRequestDto
{
    public required Uri CallbackUrl { get; init; }

    public required string AppId { get; init; }
}
