using Arbeidstilsynet.Common.FooBarKlient.Adapters;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.FooBarKlient.Test;

public class FooBarKlientTests
{
    private readonly FooBarKlientImplementation _sut;

    public FooBarKlientTests()
    {
        _sut = new FooBarKlientImplementation();
    }

    [Fact]
    public async Task Get_WhenCalled_ReturnsBar()
    {
        //arrange

        //act
        var result = await _sut.Get();
        //assert
        result.Foo.ShouldBe("Bar Bar");
    }
}
