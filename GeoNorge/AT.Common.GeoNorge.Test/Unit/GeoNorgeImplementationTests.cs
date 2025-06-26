using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.GeoNorge.Test;

public class GeoNorgeTests
{
    private readonly GeoNorgeImplementation _sut;

    public GeoNorgeTests()
    {
        _sut = new GeoNorgeImplementation();
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
