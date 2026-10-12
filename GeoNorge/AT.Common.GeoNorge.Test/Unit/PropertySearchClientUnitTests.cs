using Arbeidstilsynet.Common.GeoNorge.Eiendom;
using Arbeidstilsynet.Common.GeoNorge.Eiendom.Models;
using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using GeokodingQueryParameters = Arbeidstilsynet.Common.GeoNorge.Eiendom.Geokoding.GeokodingRequestBuilder.GeokodingRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Unit;

public class PropertySearchClientUnitTests
{
    private readonly IRequestAdapter _requestAdapter = Substitute.For<IRequestAdapter>();
    private readonly ILogger<PropertySearchClient> _logger = Substitute.For<
        ILogger<PropertySearchClient>
    >();
    private readonly GeoKodingRespons _response = new()
    {
        Type = "FeatureCollection",
        Features = [],
    };
    private readonly PropertySearchClient _sut;

    public PropertySearchClientUnitTests()
    {
        _requestAdapter.BaseUrl = "https://api.kartverket.no/eiendom/v1";
        _requestAdapter
            .SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<GeoKodingRespons>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(_response);
        _sut = new PropertySearchClient(new EiendomClient(_requestAdapter), _logger);
    }

    [Fact]
    public async Task SearchProperties_ReturnsAllFeaturesWithoutSelectingMainParcel()
    {
        _response.Features =
        [
            new() { Properties = new() { Hovedområde = false } },
            new() { Properties = new() { Hovedområde = true } },
        ];

        var result = await _sut.SearchProperties(NewQuery());

        result.ShouldBeSameAs(_response);
        var features = result.ShouldNotBeNull().Features.ShouldNotBeNull();
        features.Count.ShouldBe(2);
        features[0].Properties.ShouldNotBeNull().Hovedområde.ShouldBe(false);
        features[1].Properties.ShouldNotBeNull().Hovedområde.ShouldBe(true);
    }

    [Fact]
    public async Task SearchProperties_EmptySuccessReturnsNonNullCollectionWithoutWarning()
    {
        var result = await _sut.SearchProperties(NewQuery());

        result.ShouldBeSameAs(_response);
        result.ShouldNotBeNull().Features.ShouldNotBeNull().ShouldBeEmpty();
        _logger.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task SearchProperties_OmittedCoordinateSystemDefaultsTo4326()
    {
        var query = NewQuery();
        query.Utkoordsys.ShouldBeNull();

        await _sut.SearchProperties(query);

        query.Utkoordsys.ShouldBe(4326);
    }

    [Theory]
    [InlineData(4258)]
    [InlineData(25833)]
    public async Task SearchProperties_ExplicitCoordinateSystemIsPreserved(int coordinateSystem)
    {
        var query = NewQuery();
        query.Utkoordsys = coordinateSystem;

        await _sut.SearchProperties(query);

        query.Utkoordsys.ShouldBe(coordinateSystem);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SearchProperties_KnownFailureReturnsNullAndLogsQuery(bool apiFailure)
    {
        Exception exception = apiFailure
            ? new ApiException("API failure")
            : new HttpRequestException("Network failure");
        ThrowOnRequest(exception);
        var query = NewQuery();

        var result = await _sut.SearchProperties(query);

        result.ShouldBeNull();
        var arguments = _logger.ReceivedCalls().ShouldHaveSingleItem().GetArguments();
        arguments[0].ShouldBe(LogLevel.Warning);
        arguments[3].ShouldBeSameAs(exception);
        var state = arguments[2]
            .ShouldBeAssignableTo<IEnumerable<KeyValuePair<string, object?>>>()
            .ShouldNotBeNull();
        state.Single(pair => pair.Key == "@Query").Value.ShouldBeSameAs(query);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SearchProperties_UnexpectedFailureOrCancellationPropagates(bool cancelled)
    {
        Exception exception = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");
        ThrowOnRequest(exception);

        var actual = await Should.ThrowAsync<Exception>(() => _sut.SearchProperties(NewQuery()));

        if (cancelled)
            actual.ShouldBeAssignableTo<OperationCanceledException>();
        else
            actual.ShouldBeSameAs(exception);
        _logger.ReceivedCalls().ShouldBeEmpty();
    }

    private static GeokodingQueryParameters NewQuery() =>
        new()
        {
            Kommunenummer = "5512",
            Gardsnummer = 2,
            Bruksnummer = 16,
        };

    private void ThrowOnRequest(Exception exception) =>
        _requestAdapter
            .SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<GeoKodingRespons>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>()
            )
            .ThrowsAsync(exception);
}
