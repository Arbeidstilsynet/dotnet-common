using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
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
