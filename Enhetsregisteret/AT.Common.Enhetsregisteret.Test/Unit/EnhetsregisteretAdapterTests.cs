using Arbeidstilsynet.Common.Enhetsregisteret;
using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using EnhetGetResponse = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Enheter.Item.WithEnhetorgnrItemRequestBuilder.WithEnhetorgnrGetResponse;
using UnderenhetGetResponse = global::Arbeidstilsynet.Common.Enhetsregisteret.Enhetsregisteret.Api.Underenheter.Item.WithUnderenhetorgnrItemRequestBuilder.WithUnderenhetorgnrGetResponse;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test.Unit;

public class EnhetsregisteretAdapterTests
{
    private readonly IRequestAdapter _requestAdapter = Substitute.For<IRequestAdapter>();
    private readonly EnhetsregisteretAdapter _sut;

    public EnhetsregisteretAdapterTests()
    {
        _sut = new EnhetsregisteretAdapter(new EnhetsregisteretClient(_requestAdapter));
    }

    [Fact]
    public async Task GetEnhet_SlettetEnhet_ThrowsVirksomhetSlettetException()
    {
        // Arrange
        var slettetEnhet = new SlettetEnhet
        {
            Organisasjonsnummer = "123456789",
            Navn = "Slettet virksomhet",
            Slettedato = "2026-01-15",
        };

        SetupEnhetResponse().Returns(new EnhetGetResponse { SlettetEnhet = slettetEnhet });

        // Act
        var act = () => _sut.GetEnhet("123456789");

        // Assert
        var ex = await act.ShouldThrowAsync<VirksomhetSlettetException>();
        ex.Organisasjonsnummer.ShouldBe("123456789");
        ex.Navn.ShouldBe("Slettet virksomhet");
        ex.Slettedato.ShouldBe("2026-01-15");
        ex.SlettetVirksomhet.ShouldBeSameAs(slettetEnhet);
        ex.Message.ShouldContain("123456789");
        ex.Message.ShouldContain("Slettet virksomhet");
        ex.Message.ShouldContain("2026-01-15");
    }

    [Fact]
    public async Task GetEnhet_NotFound_ReturnsNull()
    {
        // Arrange
        SetupEnhetResponse().ThrowsAsync(new ApiException { ResponseStatusCode = 404 });

        // Act
        var result = await _sut.GetEnhet("123456789", TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetEnhet_OtherApiException_Rethrows()
    {
        // Arrange
        SetupEnhetResponse().ThrowsAsync(new ApiException { ResponseStatusCode = 500 });

        // Act
        var act = () => _sut.GetEnhet("123456789");

        // Assert
        var ex = await act.ShouldThrowAsync<ApiException>();
        ex.ResponseStatusCode.ShouldBe(500);
    }

    [Fact]
    public async Task GetUnderenhet_SlettetUnderenhet_ThrowsVirksomhetSlettetException()
    {
        // Arrange
        var slettetUnderenhet = new SlettetUnderEnhet
        {
            Organisasjonsnummer = "987654321",
            Navn = "Slettet underenhet",
            Slettedato = "2025-12-20",
        };

        SetupUnderenhetResponse().Returns(new UnderenhetGetResponse { SlettetUnderEnhet = slettetUnderenhet });

        // Act
        var act = () => _sut.GetUnderenhet("987654321");

        // Assert
        var ex = await act.ShouldThrowAsync<VirksomhetSlettetException>();
        ex.Organisasjonsnummer.ShouldBe("987654321");
        ex.Navn.ShouldBe("Slettet underenhet");
        ex.Slettedato.ShouldBe("2025-12-20");
        ex.SlettetVirksomhet.ShouldBeSameAs(slettetUnderenhet);
        ex.Message.ShouldContain("987654321");
        ex.Message.ShouldContain("Slettet underenhet");
        ex.Message.ShouldContain("2025-12-20");
    }

    [Fact]
    public async Task GetUnderenhet_NotFound_ReturnsNull()
    {
        // Arrange
        SetupUnderenhetResponse().ThrowsAsync(new ApiException { ResponseStatusCode = 404 });

        // Act
        var result = await _sut.GetUnderenhet("123456789", TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetUnderenhet_OtherApiException_Rethrows()
    {
        // Arrange
        SetupUnderenhetResponse().ThrowsAsync(new ApiException { ResponseStatusCode = 500 });

        // Act
        var act = () => _sut.GetUnderenhet("123456789");

        // Assert
        var ex = await act.ShouldThrowAsync<ApiException>();
        ex.ResponseStatusCode.ShouldBe(500);
    }

    private Task<EnhetGetResponse?> SetupEnhetResponse() =>
        _requestAdapter.SendAsync(
            Arg.Any<RequestInformation>(),
            Arg.Any<ParsableFactory<EnhetGetResponse>>(),
            Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
            Arg.Any<CancellationToken>()
        );

    private Task<UnderenhetGetResponse?> SetupUnderenhetResponse() =>
        _requestAdapter.SendAsync(
            Arg.Any<RequestInformation>(),
            Arg.Any<ParsableFactory<UnderenhetGetResponse>>(),
            Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
            Arg.Any<CancellationToken>()
        );
}
