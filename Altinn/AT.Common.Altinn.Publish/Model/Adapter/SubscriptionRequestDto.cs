namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

public record SubscriptionRequestDto
{
    public required string AltinnAppIdentifier { get; init; }
    public required Uri CallbackUrl { get; init; }
}
