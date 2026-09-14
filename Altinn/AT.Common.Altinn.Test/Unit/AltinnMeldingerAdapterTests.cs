using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class AltinnMeldingerAdapterTests
{
    private readonly IAltinnCorrespondenceClient _correspondenceClient =
        Substitute.For<IAltinnCorrespondenceClient>();
    private readonly AltinnMeldingerAdapter _sut;

    public AltinnMeldingerAdapterTests()
    {
        _sut = new AltinnMeldingerAdapter(_correspondenceClient);
    }

    [Fact]
    public async Task GetCorrespondenceByIdempotentKey_ForwardsHardcodedRoleAndResource()
    {
        //arrange
        var idempotentKey = Guid.NewGuid();
        var expected = new CorrespondenceLookupResponse { Ids = [Guid.NewGuid()] };
        _correspondenceClient
            .GetCorrespondences(
                resourceId: "dat-meldinger-correspondence",
                role: CorrespondencesRoleType.Sender,
                idempotentKey: idempotentKey
            )
            .Returns(expected);

        //act
        var result = await _sut.GetCorrespondenceByIdempotentKey(idempotentKey);

        //assert
        result.ShouldBe(expected);
        await _correspondenceClient
            .Received(1)
            .GetCorrespondences(
                resourceId: "dat-meldinger-correspondence",
                role: CorrespondencesRoleType.Sender,
                idempotentKey: idempotentKey
            );
    }
}
