using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class InstanceExtensionsTests
{
    [Fact]
    public void GetInstanceGuid_ReturnsGuid()
    {
        var guid = Guid.NewGuid();

        // Arrange
        var instance = new AltinnInstance { Id = $"dat/{guid}" };

        // Act
        var result = instance.GetInstanceGuid();

        // Assert
        result.ShouldBe(guid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("party-only")]
    [InlineData("party/not-a-guid")]
    public void GetInstanceGuid_WithInvalidId_Throws(string? id)
    {
        var instance = new AltinnInstance { Id = id };

        var exception = Should.Throw<InvalidOperationException>(() => instance.GetInstanceGuid());

        exception.Message.ShouldStartWith("AltinnInstance ID");
    }
}
