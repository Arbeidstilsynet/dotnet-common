namespace Arbeidstilsynet.Common.Altinn.Model.Adapter;

public record SubscriptionRequestDto
{
    public required Uri CallbackUrl { get; init; }
    
    public required AltinnAppSpecification AppSpecification { get; init; }
}
