using Arbeidstilsynet.Common.Enhetsregisteret.Extensions;
using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Shouldly;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test.Unit;

public class DevelopmentStubTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0.5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void SearchesRejectNonIntegerOrNonFinitePagination(double value)
    {
        var stub = new EnhetsregisteretStub();
        Should.Throw<ArgumentOutOfRangeException>(() =>
            stub.SearchEnheter(q => q.Page = value, TestContext.Current.CancellationToken)
        );
        Should.Throw<ArgumentOutOfRangeException>(() =>
            stub.SearchUnderenheter(q => q.Size = value, TestContext.Current.CancellationToken)
        );
    }

    [Fact]
    public async Task SearchesAcceptIntegerPagination()
    {
        var stub = new EnhetsregisteretStub();
        var response = await stub.SearchEnheter(
            q =>
            {
                q.Page = 0;
                q.Size = 1;
            },
            TestContext.Current.CancellationToken
        );
        response!.Embedded!.Enheter.ShouldHaveSingleItem();
        response.Page!.Number.ShouldBe(0);
        response.Page.Size.ShouldBe(1);
    }

    [Fact]
    public async Task LookupsReturnSamplesOrNullAndFreshModels()
    {
        var stub = new EnhetsregisteretStub();
        var token = TestContext.Current.CancellationToken;
        var enhet = await stub.GetEnhet("974761211", token);
        enhet!.Navn.ShouldBe("Eksempel AS");
        enhet.Navn = "modified";
        (await stub.GetEnhet("974761211", token))!.Navn.ShouldBe("Eksempel AS");
        (await stub.GetUnderenhet("974761076", token))!.Navn.ShouldBe("Eksempel AS");
        (await stub.GetEnhet("999999999", token)).ShouldBeNull();
        (await stub.GetUnderenhet("999999999", token)).ShouldBeNull();
        Should.Throw<ArgumentException>(() => stub.GetEnhet("invalid", token));
        Should.Throw<ArgumentException>(() => stub.GetUnderenhet("invalid", token));
    }

    [Fact]
    public async Task SearchesHonorSupportedFiltersAndPagination()
    {
        var stub = new EnhetsregisteretStub();
        var token = TestContext.Current.CancellationToken;
        var enheter = await stub.SearchEnheter(q => q.Navn = "eksempel", token);
        enheter!.Embedded!.Enheter.ShouldHaveSingleItem();
        enheter.Page!.TotalElements.ShouldBe(1);
        enheter.Page.TotalPages.ShouldBe(1);
        (
            await stub.SearchEnheter(q => q.Navn = "no-match", token)
        )!.Embedded!.Enheter.ShouldBeEmpty();
        (await stub.SearchEnheter(q => q.Page = 1, token))!.Embedded!.Enheter.ShouldBeEmpty();
        (
            await stub.SearchUnderenheter(q => q.OverordnetEnhet = "999999999", token)
        )!.Embedded!.Underenheter.ShouldBeEmpty();
        (await stub.GetEnheter(["974761211"])).ShouldHaveSingleItem();
        (await stub.GetUnderenheter(["974761076"])).ShouldHaveSingleItem();
        (await stub.GetUnderenheterByHovedenhet("974761211")).ShouldHaveSingleItem();
    }

    [Fact]
    public async Task UpdateHistoryIsEmptyAndPaginationTerminates()
    {
        var stub = new EnhetsregisteretStub();
        (
            await stub.GetOppdateringerEnheter(
                cancellationToken: TestContext.Current.CancellationToken
            )
        )!.Embedded!.OppdaterteEnheter.ShouldBeEmpty();
        (
            await stub.GetOppdateringerUnderenheter(
                cancellationToken: TestContext.Current.CancellationToken
            )
        )!.Embedded!.OppdaterteUnderenheter.ShouldBeEmpty();
        await foreach (var _ in stub.EnumerateOppdateringerEnheter(q => q.Dato = "2026-01-01"))
        {
            Assert.Fail("The stub has no update history.");
        }
        await foreach (
            var _ in stub.EnumerateOppdateringerUnderenheter(q => q.Oppdateringsid = "1")
        )
        {
            Assert.Fail("The stub has no update history.");
        }
    }

    [Fact]
    public void CancellationAndInvalidOrUnsupportedQueriesFailExplicitly()
    {
        var stub = new EnhetsregisteretStub();
        var token = new CancellationToken(true);
        Should.Throw<OperationCanceledException>(() => stub.GetEnhet("974761211", token));
        Should.Throw<OperationCanceledException>(() => stub.GetUnderenhet("974761076", token));
        Should.Throw<OperationCanceledException>(() =>
            stub.SearchEnheter(cancellationToken: token)
        );
        Should.Throw<OperationCanceledException>(() =>
            stub.SearchUnderenheter(cancellationToken: token)
        );
        Should.Throw<OperationCanceledException>(() =>
            stub.GetOppdateringerEnheter(cancellationToken: token)
        );
        Should.Throw<OperationCanceledException>(() =>
            stub.GetOppdateringerUnderenheter(cancellationToken: token)
        );
        Should.Throw<NotSupportedException>(() =>
            stub.SearchEnheter(
                q => q.OverordnetEnhet = "974761211",
                TestContext.Current.CancellationToken
            )
        );
        Should.Throw<ArgumentOutOfRangeException>(() =>
            stub.SearchEnheter(q => q.Page = -1, TestContext.Current.CancellationToken)
        );
        Should.Throw<ArgumentOutOfRangeException>(() =>
            stub.SearchUnderenheter(q => q.Size = 0, TestContext.Current.CancellationToken)
        );
        Should.Throw<ArgumentOutOfRangeException>(() =>
            stub.GetOppdateringerEnheter(q => q.Page = "-1", TestContext.Current.CancellationToken)
        );
        Should.Throw<FormatException>(() =>
            stub.GetOppdateringerUnderenheter(
                q => q.Size = "invalid",
                TestContext.Current.CancellationToken
            )
        );
    }
}
