using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.Altinn.Extensions.Something;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class InstanceExtensionsTests
{
    [Fact]
    public void GetInstanceGuid_ReturnsGuid()
    {
        var guid = Guid.NewGuid();

        // Arrange
        var instance = new Instance { Id = $"dat/{guid}" };

        // Act
        var result = instance.GetInstanceGuid();

        // Assert
        result.ShouldBe(guid);
    }

    [Fact]
    public void GetAppName_ReturnsAppName()
    {
        var appName = "appName";

        // Arrange
        var instance = new Instance { AppId = $"dat/{appName}" };

        // Act
        var result = instance.GetAppName();

        // Assert
        result.ShouldBe(appName);
    }

    [Fact]
    public void GetInstanceOwnerPartyId_ReturnsPartyId()
    {
        var partyId = "1337";

        // Arrange
        var instance = new Instance { InstanceOwner = new InstanceOwner { PartyId = partyId } };

        // Act
        var result = instance.GetInstanceOwnerPartyId();

        // Assert
        result.ShouldBe(int.Parse(partyId));
    }
}