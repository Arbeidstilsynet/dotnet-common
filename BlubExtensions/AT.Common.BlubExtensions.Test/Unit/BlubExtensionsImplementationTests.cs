using Arbeidstilsynet.Common.BlubExtensions.Implementation;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.BlubExtensions.Test;

public class BlubExtensionsTests
{
    private readonly BlubExtensionsImplementation _sut;

    public BlubExtensionsTests()
    {
        _sut = new BlubExtensionsImplementation();
    }

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
