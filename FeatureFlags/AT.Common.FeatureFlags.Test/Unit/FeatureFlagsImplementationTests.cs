using Arbeidstilsynet.Common.FeatureFlags.Implementation;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.FeatureFlags.Test.Unit;

public class FeatureFlagsTests
{
    private readonly FeatureFlagsImplementation _sut = new();

    [Fact]
    public async Task Get_WhenCalled_ReturnsBar()
    {
        //arrange

        //act
        var result = await _sut.Get();
        //assert
        result.Foo.ShouldBe("Bar");
    }
}
