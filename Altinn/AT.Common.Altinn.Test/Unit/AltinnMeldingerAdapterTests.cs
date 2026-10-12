using System.Reflection;
using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
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
    public void GetCorrespondenceByIdempotentKey_IsObsoleteOnInterfaceAndImplementation()
    {
        var interfaceAttribute = typeof(IAltinnMeldingerAdapter)
            .GetMethod("GetCorrespondenceByIdempotentKey")!
            .GetCustomAttribute<ObsoleteAttribute>();
        var implementationAttribute = typeof(AltinnMeldingerAdapter)
            .GetMethod("GetCorrespondenceByIdempotentKey")!
            .GetCustomAttribute<ObsoleteAttribute>();

        interfaceAttribute.ShouldNotBeNull();
        interfaceAttribute.IsError.ShouldBeFalse();
        interfaceAttribute.Message.ShouldNotBeNull();
        interfaceAttribute.Message.ShouldContain("IAltinnCorrespondenceClient.GetCorrespondences");
        interfaceAttribute.Message.ShouldContain("resourceId, role and idempotentKey");
        interfaceAttribute.Message.ShouldContain("only a small simplification");
        interfaceAttribute.Message.ShouldContain("dat-meldinger-correspondence");
        interfaceAttribute.Message.ShouldContain("team Meldinger");
        implementationAttribute.ShouldNotBeNull();
        implementationAttribute.IsError.ShouldBeFalse();
        implementationAttribute.Message.ShouldBe(interfaceAttribute.Message);
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
#pragma warning disable CS0618 // Verify the deprecated method preserves its behavior.
        var result = await _sut.GetCorrespondenceByIdempotentKey(idempotentKey);
#pragma warning restore CS0618

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
