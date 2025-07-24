using Arbeidstilsynet.Common.MeldingerReceiver.Implementation;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.MeldingerReceiver.Test;

public class MeldingerReceiverTests
{
    private readonly MeldingerReceiverImplementation _sut;

    public MeldingerReceiverTests()
    {
        _sut = new MeldingerReceiverImplementation();
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
