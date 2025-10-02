using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Validation.Extensions;
using FluentValidation;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Validation;

internal class PaginationValidator : AbstractValidator<Pagination>
{
    public PaginationValidator()
    {
        RuleFor(x => x)
            .Must(x => x.PageExtents() <= Constants.MaxSearchResultSize)
            .WithMessage($"The page extent must not exceed {Constants.MaxSearchResultSize}.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Page must be greater than or equal to 0.");
    }
}
