using System.Reflection;
using System.Text.Json.Serialization;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Model;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public record TestKeyValue
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }
    [JsonPropertyName("value")]
    public required string Value { get; init; }
}

public class EmbeddedResourceTests
{
    [Fact]
    public async Task Ingest_ShouldDeserializeJsonFromEmbeddedResource()
    {
        // Arrange
        var expectedKeyValue = new TestKeyValue { Key = "validKey", Value = "validValue" };

        // Act
        var result = await EmbeddedResource.Ingest<TestKeyValue>("valid.json");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedKeyValue);
    }
    
    [Fact]
    public async Task Ingest_ShouldThrowException_WhenResourceNotFound()
    {
        // Arrange
        var fileName = "nonexistent.json";

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () => await EmbeddedResource.Ingest<TestKeyValue>(fileName));
    }
    
    [Fact]
    public async Task Ingest_ShouldThrowException_WhenDeserializationFails()
    {
        // Arrange
        var fileName = "corrupt.json";

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () => await EmbeddedResource.Ingest<TestKeyValue>(fileName));
    }
    
    [Fact]
    public async Task Ingest_ShouldThrowException_WhenFileNameIsNotUnique()
    {
        // Arrange
        var fileName = "duplicate.json";

        // Act
        var act = async () => await EmbeddedResource.Ingest<TestKeyValue>(fileName);
        
        // Assert
        await act.ShouldThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task Ingest_ShouldUseSpecifiedAssembly_WhenProvided()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var expectedValue = new Landskode("Albania", "+355");

        // Act
        var result = await EmbeddedResource.Ingest<Landskode>("type_from_other_assembly.json", assembly);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedValue);
    }
}