using System.Reflection;
using System.Text.Json.Serialization;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public record TestKeyValue
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}

public class AssemblyExtensionsTests
{
    private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    [Fact]
    public async Task GetEmbeddedResource_ShouldDeserializeJsonFromEmbeddedResource()
    {
        // Arrange
        var expectedKeyValue = new TestKeyValue { Key = "validKey", Value = "validValue" };

        // Act
        var result = await _assembly.GetEmbeddedResource<TestKeyValue>("valid.json");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedKeyValue);
    }

    [Fact]
    public async Task GetEmbeddedResource_ShouldThrowException_WhenResourceNotFound()
    {
        // Arrange
        var fileName = "nonexistent.json";

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
            await _assembly.GetEmbeddedResource<TestKeyValue>(fileName)
        );
    }

    [Fact]
    public async Task GetEmbeddedResource_ShouldThrowException_WhenDeserializationFails()
    {
        // Arrange
        var fileName = "corrupt.json";

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
            await _assembly.GetEmbeddedResource<TestKeyValue>(fileName)
        );
    }

    [Fact]
    public async Task GetEmbeddedResource_ShouldThrowException_WhenFileNameIsNotUnique()
    {
        // Arrange
        var fileName = "duplicate.json";

        // Act
        var act = async () => await _assembly.GetEmbeddedResource<TestKeyValue>(fileName);

        // Assert
        await act.ShouldThrowAsync<Exception>();
    }
}
