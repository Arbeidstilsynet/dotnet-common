using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using FluentValidation;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Validation;

internal class GetOppdateringerQueryValidator : AbstractValidator<GetOppdateringerQuery>
{
    public GetOppdateringerQueryValidator()
    {
        RuleForEach(x => x.Organisasjonsnummer)
            .Must(orgnummer => string.IsNullOrEmpty(orgnummer) || orgnummer.Length == 9)
            .WithMessage("Each Organisasjonsnummer must be exactly 9 characters long.");
    }
}
