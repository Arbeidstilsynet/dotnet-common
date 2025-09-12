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
}
