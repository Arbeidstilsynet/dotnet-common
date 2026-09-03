using System.Reflection;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Implementation.Configuration;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Storage;
using Arbeidstilsynet.Common.Altinn.Storage.Models;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using NSubstitute;
using Shouldly;

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

        _sut = new AltinnStorageClient(
            new StorageApiClient(_requestAdapter),
            OptionsMonitorFor(
                AltinnClients.Storage,
                new AltinnClientOptions { BaseUrl = new Uri(BaseUrl) }
            )
        );
    }

    private static IOptionsMonitor<AltinnClientOptions> OptionsMonitorFor(
        string clientName,
        AltinnClientOptions options
    )
    {
        var monitor = Substitute.For<IOptionsMonitor<AltinnClientOptions>>();
        monitor.Get(clientName).Returns(options);
        return monitor;
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
    public async Task GetInstance_UsesTheGuidOnlyEndpoint()
    {
        var instanceGuid = Guid.Parse("11111111-2222-3333-4444-555555555555");

        await _sut.GetInstance(instanceGuid);

        // Altinn prefers this over the older form that also takes an instance owner party id, so
        // the party id must not appear in the path.
        CapturedUri().ShouldBe($"{BaseUrl}/instances/{instanceGuid}");
    }

    [Fact]
    public async Task GetInstance_FromCloudEvent_UsesTheGuidOnlyEndpoint()
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

        // The party id is present in the event source but is deliberately not used to address the
        // instance.
        CapturedUri().ShouldBe($"{BaseUrl}/instances/{instanceGuid}");
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
    public async Task GetInstanceData_WithNonNumericPartyId_Throws()
    {
        var act = () =>
            _sut.GetInstanceData(
                new InstanceDataRequest
                {
                    InstanceRequest = new InstanceRequest
                    {
                        InstanceOwnerPartyId = "not-a-number",
                        InstanceGuid = Guid.NewGuid(),
                    },
                    DataId = Guid.NewGuid(),
                }
            );

        // Data elements are only addressable through the older two-segment path, whose generated
        // builder indexes by an integer party id, so this has to fail loudly rather than silently
        // address the wrong resource.
        await Should.ThrowAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetInstances_AppliesInstanceOwnerIdentifierHeader()
    {
        await _sut.GetInstances(
            new InstanceQueryParameters { InstanceOwnerIdentifier = "0192:123456789" }
        );

        // Kiota omits header parameters from the generated query-parameter class, which is the
        // reason InstanceQueryParameters carries it separately. Without that this header would be
        // dropped silently.
        var headers = CapturedRequest().Headers;

        headers
            .ContainsKey(InstanceQueryParameters.InstanceOwnerIdentifierHeaderName)
            .ShouldBeTrue();
        headers[InstanceQueryParameters.InstanceOwnerIdentifierHeaderName]
            .ShouldContain("0192:123456789");
    }

    [Fact]
    public async Task GetInstances_WithoutInstanceOwnerIdentifier_OmitsHeader()
    {
        await _sut.GetInstances(new InstanceQueryParameters { AppId = "dat/some-app" });

        CapturedRequest()
            .Headers.ContainsKey(InstanceQueryParameters.InstanceOwnerIdentifierHeaderName)
            .ShouldBeFalse();
    }

    [Fact]
    public async Task GetInstances_MapsQueryParametersOntoTheWire()
    {
        await _sut.GetInstances(
            new InstanceQueryParameters
            {
                Org = "dat",
                AppId = "dat/some-app",
                ProcessIsComplete = true,
                ProcessCurrentTask = "Task_1",
                ExcludeConfirmedBy = "dat",
                IsArchived = false,
                Size = 50,
                SortBy = "desc:lastChanged",
                SearchString = "needle",
                MainVersionInclude = 3,
            }
        );

        var uri = CapturedUri();

        // The dotted parameters are the easy ones to get wrong: the generated property names drop
        // the dots, so only the serialised URI proves the wire name survived. IsArchived and SortBy
        // are also renamed on the way through (StatusIsArchived and Order).
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
            new InstanceQueryParameters
            {
                LastChanged =
                [
                    new AltinnDateTimeQuery
                    {
                        CompareOperator = DateTimeCompareOperator.gte,
                        DateTime = "2024-01-01",
                    },
                    new AltinnDateTimeQuery
                    {
                        CompareOperator = DateTimeCompareOperator.lt,
                        DateTime = "2024-02-01",
                    },
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
        var parameters = new InstanceQueryParameters { AppId = "dat/some-app" };

        parameters
            .TryAppendContinuationToken(
                new Uri("https://example.com?continuationToken=token-from-next-page"),
                out var nextPageParameters
            )
            .ShouldBeTrue();

        await _sut.GetInstances(nextPageParameters);

        // Paging must not write the continuation token back into the object the caller passed in.
        parameters.ContinuationToken.ShouldBeNull();
        CapturedUri().ShouldContain("continuationToken=token-from-next-page");
    }
}
