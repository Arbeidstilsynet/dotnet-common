using Altinn.App.Core.Models;
using Arbeidstilsynet.Common.Altinn.Implementation;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class HttpExtensionsTests
{
    private readonly IHttpRequestBuilder _httpRequestBuilderMock;

    private static AltinnDateTimeQuery[] SampleAltinnDateTimeQueryList =
    [
        new AltinnDateTimeQuery
        {
            CompareOperator = DateTimeCompareOperator.gt,
            DateTime = "2025-07-01",
        },
        new AltinnDateTimeQuery
        {
            CompareOperator = DateTimeCompareOperator.lt,
            DateTime = "2025-07-18",
        },
    ];

    private static string SampleString = "example-string";

    private static int SampleInt = 42;

    private static bool SampleBool = true;
    private static readonly InstanceQueryParameters InstanceQueryParameterComplete =
        new InstanceQueryParameters
        {
            Org = SampleString,
            AppId = SampleString,
            ProcessCurrentTask = SampleString,
            ProcessIsComplete = SampleBool,
            ProcessEndEvent = SampleString,
            ProcessEnded = SampleAltinnDateTimeQueryList,
            InstanceOwnerPartyId = SampleInt,
            LastChanged = SampleAltinnDateTimeQueryList,
            Created = SampleAltinnDateTimeQueryList,
            VisibleAfter = SampleAltinnDateTimeQueryList,
            DueBefore = SampleAltinnDateTimeQueryList,
            ExcludeConfirmedBy = SampleString,
            Confirmed = SampleBool,
            IsSoftDeleted = SampleBool,
            IsHardDeleted = SampleBool,
            IsArchived = SampleBool,
            ContinuationToken = SampleString,
            Size = SampleInt,
            InstanceOwnerIdentifier = SampleString,
            MainVersionInclude = SampleInt,
            MainVersionExclude = SampleInt,
            SearchString = SampleString,
            SortBy = SampleString,
        };

    public HttpExtensionsTests()
    {
        _httpRequestBuilderMock = Substitute.For<IHttpRequestBuilder>();
        _httpRequestBuilderMock
            .WithQueryParameter(Arg.Any<string>(), Arg.Any<string>())
            .Returns(_httpRequestBuilderMock);
        _httpRequestBuilderMock
            .WithHeader(Arg.Any<string>(), Arg.Any<string>())
            .Returns(_httpRequestBuilderMock);
    }

    [Theory]
    [InlineData(
        "https://ttd.apps.at22.altinn.cloud/ttd/apps-test/instances/50019855/6b3323c8-7baf-4612-b8a6-5eac407f4d0c"
    )]
    [InlineData(
        "https://ttd.apps.at22.altinn.cloud/ttd/apps-test/instances/50019855/6b3323c8-7baf-4612-b8a6-5eac407f4d0c?foo=bar"
    )]
    [InlineData(
        "https://ttd.apps.at22.altinn.cloud/ttd/apps-test/instances/50019855/6b3323c8-7baf-4612-b8a6-5eac407f4d0c/anotherPath"
    )]
    [InlineData(
        "https://ttd.apps.at22.altinn.cloud/ttd/apps-test/instances/50019855/6b3323c8-7baf-4612-b8a6-5eac407f4d0c/anotherPath?foo=bar"
    )]
    public void CloudEventToInstanceUri_WhenCalledWithExpectedSource_ShouldBeParsedCorrectly(
        string validUrl
    )
    {
        //arrange
        var sut = new CloudEvent { Source = new Uri(validUrl) };
        //act
        var result = sut.ToInstanceUri();
        //assert
        result.ToString().ShouldBe("instances/50019855/6b3323c8-7baf-4612-b8a6-5eac407f4d0c");
    }

    [Fact]
    public void CloudEventToInstanceUri_WhenCalledWithUnexpectedSource_ThrowsException()
    {
        //arrange
        var sut = new CloudEvent
        {
            Source = new Uri(
                "https://ttd.apps.at22.altinn.cloud/ttd/apps-test/50019855/6b3323c8-7baf-4612-b8a6-5eac407f4d0c?foo=bar"
            ),
        };
        //act
        var result = () => sut.ToInstanceUri();
        //assert
        result.ShouldThrow<AltinnEventSourceParseException>();
    }

    [Fact]
    public void InstanceRequestToInstanceUri_WhenCalledWithExpectedSource_ShouldBeParsedCorrectly()
    {
        //arrange
        var sut = new InstanceRequest
        {
            InstanceGuid = Guid.Parse("6b3323c8-7baf-4612-b8a6-5eac407f4d0c"),
            InstanceOwnerPartyId = "50019855",
        };
        //act
        var result = sut.ToInstanceUri();
        //assert
        result.ToString().ShouldBe("instances/50019855/6b3323c8-7baf-4612-b8a6-5eac407f4d0c");
    }

    [Fact]
    public void InstanceQueryParameters_ApplyToBuilderComplete_MapsAllFields()
    {
        //arrange
        _httpRequestBuilderMock.ClearReceivedCalls();
        var queryParameter = InstanceQueryParameterComplete;
        //act
        _httpRequestBuilderMock.ApplyInstanceQueryParameters(queryParameter);
        //assert
        _httpRequestBuilderMock.Received(1).WithQueryParameter("org", SampleString);
        _httpRequestBuilderMock.Received(1).WithQueryParameter("appId", SampleString);
        _httpRequestBuilderMock.Received(1).WithQueryParameter("process.currentTask", SampleString);
        _httpRequestBuilderMock
            .Received(1)
            .WithQueryParameter("process.isComplete", SampleBool.ToString());
        _httpRequestBuilderMock.Received(1).WithQueryParameter("process.endEvent", SampleString);
        _httpRequestBuilderMock.Received(1).WithQueryParameter("process.ended", "gt:2025-07-01");
        _httpRequestBuilderMock.Received(1).WithQueryParameter("process.ended", "lt:2025-07-18");
        _httpRequestBuilderMock
            .Received(1)
            .WithQueryParameter("instanceOwner.partyId", SampleInt.ToString());
        _httpRequestBuilderMock.Received(1).WithQueryParameter("lastChanged", "gt:2025-07-01");
        _httpRequestBuilderMock.Received(1).WithQueryParameter("lastChanged", "lt:2025-07-18");
        _httpRequestBuilderMock.Received(1).WithQueryParameter("created", "gt:2025-07-01");
        _httpRequestBuilderMock.Received(1).WithQueryParameter("created", "lt:2025-07-18");
        _httpRequestBuilderMock.Received(1).WithQueryParameter("visibleAfter", "gt:2025-07-01");
        _httpRequestBuilderMock.Received(1).WithQueryParameter("visibleAfter", "lt:2025-07-18");
        _httpRequestBuilderMock.Received(1).WithQueryParameter("dueBefore", "gt:2025-07-01");
        _httpRequestBuilderMock.Received(1).WithQueryParameter("dueBefore", "lt:2025-07-18");
        _httpRequestBuilderMock.Received(1).WithQueryParameter("excludeConfirmedBy", SampleString);
        _httpRequestBuilderMock.Received(1).WithQueryParameter("confirmed", SampleBool.ToString());
        _httpRequestBuilderMock
            .Received(1)
            .WithQueryParameter("status.isSoftDeleted", SampleBool.ToString());
        _httpRequestBuilderMock
            .Received(1)
            .WithQueryParameter("status.isHardDeleted", SampleBool.ToString());
        _httpRequestBuilderMock
            .Received(1)
            .WithQueryParameter("status.isArchived", SampleBool.ToString());
        _httpRequestBuilderMock.Received(1).WithQueryParameter("continuationToken", SampleString);
        _httpRequestBuilderMock.Received(1).WithQueryParameter("size", SampleInt.ToString());
        _httpRequestBuilderMock
            .Received(1)
            .WithQueryParameter("mainVersionInclude", SampleInt.ToString());
        _httpRequestBuilderMock
            .Received(1)
            .WithQueryParameter("mainVersionExclude", SampleInt.ToString());
        _httpRequestBuilderMock.Received(1).WithQueryParameter("searchString", SampleString);
        _httpRequestBuilderMock.Received(1).WithQueryParameter("order", SampleString);
        _httpRequestBuilderMock
            .Received(1)
            .WithHeader("X-Ai-InstanceOwnerIdentifier", SampleString);
    }

    [Fact]
    public void InstanceQueryParameters_ApplyToBuilderPartly_MapsOnlyNecessaryFields()
    {
        //arrange
        _httpRequestBuilderMock.ClearReceivedCalls();
        var queryParameter = new InstanceQueryParameters
        {
            AppId = "dat/test",
            Org = "dat",
            ProcessIsComplete = true,
            ExcludeConfirmedBy = "dat",
        };
        //act
        _httpRequestBuilderMock.ApplyInstanceQueryParameters(queryParameter);
        //assert
        _httpRequestBuilderMock.Received(1).WithQueryParameter("appId", "dat/test");
        _httpRequestBuilderMock.Received(1).WithQueryParameter("org", "dat");
        _httpRequestBuilderMock
            .Received(1)
            .WithQueryParameter("process.isComplete", SampleBool.ToString());
        _httpRequestBuilderMock.Received(1).WithQueryParameter("excludeConfirmedBy", "dat");
        _httpRequestBuilderMock
            .Received(4)
            .WithQueryParameter(Arg.Any<string>(), Arg.Any<string>());
        _httpRequestBuilderMock.DidNotReceive().WithHeader(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void InstanceQueryParameters_ApplyToBuilderEmpty_MapsNoFields()
    {
        //arrange
        _httpRequestBuilderMock.ClearReceivedCalls();
        var queryParameter = new InstanceQueryParameters { };
        //act
        _httpRequestBuilderMock.ApplyInstanceQueryParameters(queryParameter);
        //assert
        _httpRequestBuilderMock.DidNotReceiveWithAnyArgs().WithQueryParameter(default, default);
        _httpRequestBuilderMock.DidNotReceiveWithAnyArgs().WithHeader(default, default);
    }
}
