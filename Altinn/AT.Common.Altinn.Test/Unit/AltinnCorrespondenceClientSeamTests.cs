using Arbeidstilsynet.Common.Altinn.Correspondence;
using Arbeidstilsynet.Common.Altinn.Correspondence.Models;
using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using NSubstitute;
using Shouldly;
using CorrespondencesRoleType = Arbeidstilsynet.Common.Altinn.Model.Api.Request.CorrespondencesRoleType;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class AltinnCorrespondenceClientSeamTests
{
    private const string BaseUrl = "https://platform.tt02.altinn.no";

    private readonly IRequestAdapter _requestAdapter = Substitute.For<IRequestAdapter>();
    private readonly AltinnCorrespondenceClient _sut;

    public AltinnCorrespondenceClientSeamTests()
    {
        _requestAdapter.BaseUrl = BaseUrl;
        _requestAdapter
            .SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<CorrespondencesExt>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                new CorrespondencesExt
                {
                    Ids = [Guid.Parse("11111111-2222-3333-4444-555555555555"), null],
                }
            );

        _sut = new AltinnCorrespondenceClient(new CorrespondenceApiClient(_requestAdapter), null!);
    }

    [Fact]
    public async Task GetCorrespondences_MapsQueryAndResponse()
    {
        var idempotentKey = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        using var cancellation = new CancellationTokenSource();

        var result = await _sut.GetCorrespondences(
            resourceId: "dat-meldinger-correspondence",
            from: new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
            to: new DateTimeOffset(2026, 2, 3, 4, 5, 6, TimeSpan.Zero),
            status: Model.Api.Response.CorrespondenceStatus.Published,
            role: CorrespondencesRoleType.Sender,
            onBehalfOf: "urn:altinn:organization:identifier-no:123456789",
            sendersReference: "reference",
            idempotentKey: idempotentKey,
            altinn2CorrespondenceId: 42,
            cancellationToken: cancellation.Token
        );

        result.Ids.ShouldBe([Guid.Parse("11111111-2222-3333-4444-555555555555")]);

        var call = _requestAdapter
            .ReceivedCalls()
            .Single(received => received.GetArguments().FirstOrDefault() is RequestInformation);
        var request = (RequestInformation)call.GetArguments()[0]!;
        var uri = request.URI.ToString();

        uri.ShouldStartWith($"{BaseUrl}/correspondence/api/v1/correspondence?");
        uri.ShouldContain("resourceId=dat-meldinger-correspondence");
        uri.ShouldContain("status=Published");
        uri.ShouldContain("role=Sender");
        uri.ShouldContain($"idempotentKey={idempotentKey}");
        uri.ShouldContain("altinn2CorrespondenceId=42");
        call.GetArguments()[3].ShouldBe(cancellation.Token);
    }
}
