using Altinn.App.Core.Models;
using Arbeidstilsynet.Common.Altinn.Implementation;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;
using NSubstitute.Core;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class HttpExtensionsTests
{
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
}
