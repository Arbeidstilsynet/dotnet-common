using Arbeidstilsynet.Common.MeldingerReceiver.Extensions;
using Arbeidstilsynet.Common.MeldingerReceiver.Extensions.Something;
using Arbeidstilsynet.Common.MeldingerReceiver.Model;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.MeldingerReceiver.Test;

public class MeldingerReceiverExtensionsTests
{
    [Fact]
    public void MeldingerReceiverExtensions_ShouldHaveCorrectNamespace()
    {
        // Arrange
        var model = new MeldingerReceiverDto() { Foo = "Bar" };

        // Act
        var result = model.ToUpper();

        // Assert
        result.Foo.ShouldBe("BAR");
    }
}
