using Arbeidstilsynet.Common.GeoNorge.Extensions;
using Arbeidstilsynet.Common.GeoNorge.Extensions.Something;
using Arbeidstilsynet.Common.GeoNorge.Model;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.GeoNorge.Test;

public class GeoNorgeExtensionsTests
{
    [Fact]
    public void GeoNorgeExtensions_ShouldHaveCorrectNamespace()
    {
        // Arrange
        var model = new GeoNorgeDto() { Foo = "Bar" };

        // Act
        var result = model.ToUpper();

        // Assert
        result.Foo.ShouldBe("BAR");
    }
}
