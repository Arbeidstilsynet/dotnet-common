using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Validation;
using Shouldly;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test;

public class ValidatorTests
{
    private readonly SearchEnheterQueryValidator _searchEnheterQueryValidator = new();
    private readonly PaginationValidator _paginationValidator = new();

    [Fact]
    public void SearchEnheterQueryValidator_NavnAtMaxLength_IsValid()
    {
        var query = new SearchEnheterQuery
        {
            Navn = new string('a', Constants.MaxSearchStringLength),
        };

        var result = _searchEnheterQueryValidator.Validate(query);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void SearchEnheterQueryValidator_NavnExceedingMaxLength_IsInvalid()
    {
        var query = new SearchEnheterQuery
        {
            Navn = new string('a', Constants.MaxSearchStringLength + 1),
        };

        var result = _searchEnheterQueryValidator.Validate(query);

        result.IsValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0, Constants.MaxSearchResultSize)] // extent == max
    [InlineData(1, Constants.MaxSearchResultSize / 2)] // extent == max
    public void PaginationValidator_PageExtentAtMax_IsValid(long page, long size)
    {
        var pagination = new Pagination { Page = page, Size = size };

        var result = _paginationValidator.Validate(pagination);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void PaginationValidator_PageExtentExceedingMax_IsInvalid()
    {
        var pagination = new Pagination { Page = 0, Size = Constants.MaxSearchResultSize + 1 };

        var result = _paginationValidator.Validate(pagination);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void PaginationValidator_NegativePage_IsInvalid()
    {
        var pagination = new Pagination { Page = -1, Size = 10 };

        var result = _paginationValidator.Validate(pagination);

        result.IsValid.ShouldBeFalse();
    }
}
