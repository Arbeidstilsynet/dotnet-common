using System.Reflection;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Storage;
using Arbeidstilsynet.Common.Altinn.Storage.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using NSubstitute;
using Shouldly;
using GeneratedInstanceQueryParameters = Arbeidstilsynet.Common.Altinn.Storage.Instances.InstancesRequestBuilder.InstancesRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

/// <summary>
/// Covers the seam between the package's own request models and the generated storage client, by
/// substituting the request adapter and asserting on the <see cref="RequestInformation"/> the
/// client produces.
/// </summary>
public class AltinnStorageClientSeamTests
{
    private const string BaseUrl = "https://platform.tt02.altinn.no/storage/api/v1";

    private readonly IRequestAdapter _requestAdapter = Substitute.For<IRequestAdapter>();
    private readonly AltinnStorageClient _sut;

    public AltinnStorageClientSeamTests()
    {
        _requestAdapter.BaseUrl = BaseUrl;
        _requestAdapter
            .SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<InstanceQueryResponse>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new InstanceQueryResponse());
        _requestAdapter
            .SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<Instance>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new Instance());
        _requestAdapter
            .SendPrimitiveAsync<Stream>(
                Arg.Any<RequestInformation>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new MemoryStream());

        _sut = new AltinnStorageClient(new StorageApiClient(_requestAdapter));
    }

    private RequestInformation CapturedRequest()
    {
        var call = _requestAdapter
            .ReceivedCalls()
            .LastOrDefault(c => c.GetArguments().FirstOrDefault() is RequestInformation);

        call.ShouldNotBeNull("no request was sent to the request adapter");

        return (RequestInformation)call.GetArguments()[0]!;
    }

    private string CapturedUri() => CapturedRequest().URI.ToString();

    [Fact]
    public async Task GetInstance_AddressesInstanceByPartyIdAndGuid()
    {
        var instanceGuid = Guid.Parse("11111111-2222-3333-4444-555555555555");

        await _sut.GetInstance(
            new InstanceRequest { InstanceOwnerPartyId = "51644866", InstanceGuid = instanceGuid }
        );

        CapturedUri().ShouldBe($"{BaseUrl}/instances/51644866/{instanceGuid}");
    }

    [Fact]
    public async Task GetInstance_FromCloudEvent_AddressesInstanceByPartyIdAndGuid()
    {
        var instanceGuid = Guid.Parse("11111111-2222-3333-4444-555555555555");

        await _sut.GetInstance(
            new AltinnCloudEvent
            {
                Source = new Uri(
                    $"https://dat.apps.altinn.no/dat/some-app/instances/51644866/{instanceGuid}"
                ),
            }
        );

        CapturedUri().ShouldBe($"{BaseUrl}/instances/51644866/{instanceGuid}");
    }

    [Fact]
    public async Task GetInstanceData_AddressesDataElement()
    {
        var instanceGuid = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var dataGuid = Guid.Parse("66666666-7777-8888-9999-000000000000");

        await _sut.GetInstanceData(
            new InstanceDataRequest
            {
                InstanceRequest = new InstanceRequest
                {
                    InstanceOwnerPartyId = "51644866",
                    InstanceGuid = instanceGuid,
                },
                DataId = dataGuid,
            }
        );

        CapturedUri().ShouldBe($"{BaseUrl}/instances/51644866/{instanceGuid}/data/{dataGuid}");
    }

    [Fact]
    public async Task GetInstanceData_ByAbsoluteUri_UsesThatUriVerbatim()
    {
        var absoluteUri = new Uri(
            "https://platform.tt02.altinn.no/storage/api/v1/instances/1/2b6b2f1c-0000-0000-0000-000000000000/data/3c7c3f2d-0000-0000-0000-000000000000"
        );

        await _sut.GetInstanceData(absoluteUri);

        CapturedUri().ShouldBe(absoluteUri.ToString());
    }

    [Fact]
    public async Task GetInstance_WithNonNumericPartyId_Throws()
    {
        var act = () =>
            _sut.GetInstance(
                new InstanceRequest
                {
                    InstanceOwnerPartyId = "not-a-number",
                    InstanceGuid = Guid.NewGuid(),
                }
            );

        // The generated client indexes instances by an integer, so this has to fail loudly rather
        // than silently address the wrong resource.
        await Should.ThrowAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetInstances_AppliesInstanceOwnerIdentifierHeader()
    {
        await _sut.GetInstances(new InstanceQuery { InstanceOwnerIdentifier = "0192:123456789" });

        // Kiota omits header parameters from the generated query-parameter class, which is the
        // reason InstanceQuery exists. Without it this header would be dropped silently.
        var headers = CapturedRequest().Headers;

        headers.ContainsKey(InstanceQuery.InstanceOwnerIdentifierHeaderName).ShouldBeTrue();
        headers[InstanceQuery.InstanceOwnerIdentifierHeaderName].ShouldContain("0192:123456789");
    }

    [Fact]
    public async Task GetInstances_WithoutInstanceOwnerIdentifier_OmitsHeader()
    {
        await _sut.GetInstances(new GeneratedInstanceQueryParameters { AppId = "dat/some-app" });

        CapturedRequest()
            .Headers.ContainsKey(InstanceQuery.InstanceOwnerIdentifierHeaderName)
            .ShouldBeFalse();
    }

    [Fact]
    public async Task GetInstances_MapsQueryParametersOntoTheWire()
    {
        await _sut.GetInstances(
            new GeneratedInstanceQueryParameters
            {
                Org = "dat",
                AppId = "dat/some-app",
                ProcessIsComplete = true,
                ProcessCurrentTask = "Task_1",
                ExcludeConfirmedBy = "dat",
                StatusIsArchived = false,
                Size = 50,
                Order = "desc:lastChanged",
                SearchString = "needle",
                MainVersionInclude = 3,
            }
        );

        var uri = CapturedUri();

        // The dotted parameters are the easy ones to get wrong: the generated property names drop
        // the dots, so only the serialised URI proves the wire name survived.
        uri.ShouldContain("org=dat");
        uri.ShouldContain("process.isComplete=true");
        uri.ShouldContain("process.currentTask=Task_1");
        uri.ShouldContain("status.isArchived=false");
        uri.ShouldContain("excludeConfirmedBy=dat");
        uri.ShouldContain("size=50");
        uri.ShouldContain("searchString=needle");
        uri.ShouldContain("mainVersionInclude=3");
        uri.ShouldContain("order=desc%3AlastChanged");
    }

    [Fact]
    public async Task GetInstances_MapsDateRangeQueriesToComparisonExpressions()
    {
        await _sut.GetInstances(
            new GeneratedInstanceQueryParameters
            {
                LastChanged =
                [
                    AltinnDateTimeQuery.GreaterThanOrEquals(
                        new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
                    ),
                    AltinnDateTimeQuery.LessThan(
                        new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero)
                    ),
                ],
            }
        );

        var uri = CapturedUri();

        uri.ShouldContain("gte%3A2024-01-01");
        uri.ShouldContain("lt%3A2024-02-01");
    }

    [Fact]
    public async Task GetInstances_DoesNotMutateTheCallersParameters()
    {
        var parameters = new GeneratedInstanceQueryParameters { AppId = "dat/some-app" };
        var query = new InstanceQuery { Parameters = parameters };

        await _sut.GetInstances(query.WithContinuationToken("token-from-next-page"));

        // Paging must not write the continuation token back into the object the caller passed in.
        parameters.ContinuationToken.ShouldBeNull();
        CapturedUri().ShouldContain("continuationToken=token-from-next-page");
    }
}
