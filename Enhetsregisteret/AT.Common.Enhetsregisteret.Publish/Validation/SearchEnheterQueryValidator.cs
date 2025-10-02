using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Validation.Extensions;
using FluentValidation;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Validation;

internal class SearchEnheterQueryValidator : AbstractValidator<SearchEnheterQuery>
{
    public SearchEnheterQueryValidator()
    {
        RuleFor(x => x.Navn)
            .MaximumLength(Constants.MaxSearchStringLength)
            .WithMessage($"Navn must not exceed {Constants.MaxSearchStringLength} characters.");

        RuleForEach(x => x.Organisasjonsnummer)
            .Must(orgnummer => orgnummer.IsValidOrgnummer())
            .WithMessage("Each Organisasjonsnummer must be exactly 9 characters long.");

        RuleFor(x => x.OverordnetEnhetOrganisasjonsnummer)
            .Must(orgnummer => orgnummer.IsValidOrgnummer())
            .WithMessage(
                "OverordnetEnhetOrganisasjonsnummer must be exactly 9 characters long if provided."
            );
    }
}
