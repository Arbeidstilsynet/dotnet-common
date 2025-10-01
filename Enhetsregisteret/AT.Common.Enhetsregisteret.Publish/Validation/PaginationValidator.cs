using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using FluentValidation;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Validation;

internal class PaginationValidator : AbstractValidator<Pagination>
{
    public PaginationValidator()
    {
        RuleFor(x => x)
            .Must(model => model.Page+1 * model.Size <= Constants.MaxSearchResultSize)
            .WithMessage($"The product of (Page + 1) and Size must not exceed {Constants.MaxSearchResultSize}.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0).WithMessage("Page must be greater than or equal to 0.");
    }
}