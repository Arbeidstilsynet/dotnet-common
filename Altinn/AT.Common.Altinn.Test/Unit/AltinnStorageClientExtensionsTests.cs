using Altinn.App.Core.Models;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class AltinnStorageClientExtensionsTests
{
    private readonly IAltinnStorageClient _client = Substitute.For<IAltinnStorageClient>();

    [Fact]
    public async Task GetAllInstances_WhenCalledWithValidQueryParameters_ReturnsAllInstances()
    {
        // Arrange
        var queryParameters = new InstanceQueryParameters { AppId = "my-app" };

        var firstPage = new QueryResponse<Instance>
        {
            Instances = [new Instance { Id = "1" }, new Instance { Id = "2" }],
            Next = "http://example.com?continuationToken=abc123",
        };

        var secondPage = new QueryResponse<Instance>
        {
            Instances = [new Instance { Id = "3" }],
            Next = null,
        };

        _client.GetInstances(queryParameters).Returns(firstPage);
        _client
            .GetInstances(queryParameters with { ContinuationToken = "abc123" })
            .Returns(secondPage);

        // Act
        var result = (await _client.GetAllInstances(queryParameters)).ToList();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(firstPage.Instances.Concat(secondPage.Instances));
    }

    [Fact]
    public async Task GetAllInstances_NextLinkLoops_IgnoresIt()
    {
        // Arrange
        var queryParameters = new InstanceQueryParameters { AppId = "my-app" };

        var firstPage = new QueryResponse<Instance>
        {
            Instances = [new Instance { Id = "1" }, new Instance { Id = "2" }],
            Next = "http://example.com?continuationToken=abc123",
        };

        var secondPage = new QueryResponse<Instance>
        {
            Instances = [new Instance { Id = "3" }],
            Next = "http://example.com?continuationToken=abc123",
        };

        _client.GetInstances(queryParameters).Returns(firstPage);
        _client
            .GetInstances(queryParameters with { ContinuationToken = "abc123" })
            .Returns(secondPage);

        // Act
        var result = (await _client.GetAllInstances(queryParameters)).ToList();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(firstPage.Instances.Concat(secondPage.Instances));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("/relative/path?continuationToken=abc123")]
    [InlineData("invalid-url")]
    public async Task GetAllInstances_WhenNextLinkIsInvalidUrl_OnlyReturnsFirstPage(
        string? nextLink
    )
    {
        // Arrange
        var queryParameters = new InstanceQueryParameters { AppId = "my-app" };

        var firstPage = new QueryResponse<Instance>
        {
            Instances = [new Instance { Id = "1" }, new Instance { Id = "2" }],
            Next = nextLink,
        };

        var secondPage = new QueryResponse<Instance>
        {
            Instances = [new Instance { Id = "3" }],
            Next = null,
        };

        _client.GetInstances(queryParameters).Returns(firstPage);
        _client
            .GetInstances(queryParameters with { ContinuationToken = "abc123" })
            .Returns(secondPage);

        // Act
        var result = (await _client.GetAllInstances(queryParameters)).ToList();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(firstPage.Instances);
    }
}
