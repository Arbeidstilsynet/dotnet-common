using Arbeidstilsynet.Common.BlubExtensions.Extensions;
using Arbeidstilsynet.Common.BlubExtensions.Extensions.Something;
using Arbeidstilsynet.Common.BlubExtensions.Model;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.BlubExtensions.Test;

public class BlubExtensionsExtensionsTests
{
    [Fact]
    public void BlubExtensionsExtensions_ShouldHaveCorrectNamespace()
    {
        // Arrange
        var model = new BlubExtensionsDto() { Foo = "Bar" };

        // Act
        var result = model.ToUpper();

        // Assert
        result.Foo.ShouldBe("BAR");
    }
}
