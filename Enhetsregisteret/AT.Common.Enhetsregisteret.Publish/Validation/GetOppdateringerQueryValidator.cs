using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Validation.Extensions;
using FluentValidation;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Validation;

internal class GetOppdateringerQueryValidator : AbstractValidator<GetOppdateringerQuery>
{
    public GetOppdateringerQueryValidator()
    {
        RuleForEach(x => x.Organisasjonsnummer)
            .Must(orgnummer => orgnummer.IsValidOrgnummer())
            .WithMessage("Each Organisasjonsnummer must be exactly 9 characters long.");
    }
}
