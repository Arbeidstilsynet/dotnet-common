using System.ComponentModel.DataAnnotations;
using Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Test.Unit;

public class ConfigurationExtensionsTests
{
    [Fact]
    public void GetRequired_ValidConfiguration_ShouldNotThrow()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>() { { "Nested:Nested:RenamedString", "Test" } }
            )
            .Build();

        // Act
        var act = () => configuration.GetRequired<TestAppSettings>();

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void GetRequired_MissingRequiredField_ShouldThrow_WithMessage()
    {
        // Arrange
        var invalidConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>() { { "Nested:Nested:RenamedString", null } }
            )
            .Build();

        // Act
        var act = () => invalidConfiguration.GetRequired<TestAppSettings>();

        // Assert
        var exception = act.ShouldThrow<InvalidOperationException>();

        exception.Message.ShouldContain("Invalid configuration for `TestAppSettings`");
        exception.Message.ShouldContain("Section: Nested:Nested");
        exception.Message.ShouldContain("The RenamedString field is required");
    }

    [Fact]
    public void GetRequired_FieldOfWrongType_ShouldThrow_WithMessage()
    {
        // Arrange
        var invalidConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>()
                {
                    { "Nested:Nested:RenamedString", "OK" },
                    { "Nested:RenamedInt", "NotAnInt" },
                }
            )
            .Build();

        // Act
        var act = () => invalidConfiguration.GetRequired<TestAppSettings>();

        // Assert
        var exception = act.ShouldThrow<InvalidOperationException>();

        exception.Message.ShouldContain(
            "Failed to convert configuration value 'NotAnInt' at 'Nested:RenamedInt' to type 'System.Int32'."
        );
    }

    [Fact]
    public void GetRequired_ComplexConfiguration_ShouldReturnConfig()
    {
        var uri = new Uri("https://example.com/");
        var timeSpan = TimeSpan.FromSeconds(1);

        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>()
                {
                    { "Nested:Nested:RenamedString", "Test" },
                    { "Complex:Int", "1" },
                    { "Complex:Decimal", "1.02" },
                    { "Complex:String", "Test" },
                    { "Complex:Bool", "true" },
                    { "Complex:Uri", uri.ToString() },
                    { "Complex:TimeSpan", timeSpan.ToString() },
                    { "Complex:StringArray:0", "Test1" },
                    { "Complex:StringArray:1", "Test2" },
                    { "Complex:StringList:0", "Test3" },
                    { "Complex:StringList:1", "Test4" },
                }
            )
            .Build();

        // Act
        var appSettings = configuration.GetRequired<TestAppSettings>();

        // Assert
        appSettings.ShouldBeEquivalentTo(
            new TestAppSettings()
            {
                Nested = new DoubleNestedConfig()
                {
                    Nested = new NestedConfig() { NotExposedFieldName = "Test" },
                },
                Complex = new ComplexConfig()
                {
                    Int = 1,
                    Decimal = 1.02m,
                    String = "Test",
                    Bool = true,
                    TimeSpan = timeSpan,
                    Uri = uri,
                    StringArray = ["Test1", "Test2"],
                    StringList = ["Test3", "Test4"],
                },
            }
        );
    }

    [Fact]
    public void GetRequired_InfiniteLoop_ShouldNotThrow()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>() { { "Nested", "{}" } })
            .Build();

        // Act
        var act = () => configuration.GetRequired<InfiniteLoopConfig>();

        // Assert
        act.ShouldNotThrow();
    }

    private class InfiniteLoopConfig
    {
        public InfiniteLoopConfig? Self => this;
    }

    private class TestAppSettings
    {
        public string? NotRequired { get; set; }

        [Required]
        public required DoubleNestedConfig Nested { get; set; }

        public ComplexConfig? Complex { get; set; }

        public int? Int { get; set; }
        public int? Int2 { get; set; }
    }

    private class DoubleNestedConfig
    {
        [Required]
        public required NestedConfig Nested { get; set; }

        [ConfigurationKeyName("RenamedInt")]
        public int Int { get; set; }
    }

    private class NestedConfig
    {
        [Required]
        [ConfigurationKeyName("RenamedString")]
        public required string NotExposedFieldName { get; set; }
    }

    private class ComplexConfig
    {
        public int? Int { get; set; }
        public decimal? Decimal { get; set; }
        public string? String { get; set; }
        public bool? Bool { get; set; }
        public TimeSpan? TimeSpan { get; set; }
        public Uri? Uri { get; set; }
        public string[]? StringArray { get; set; }
        public List<string>? StringList { get; set; }
    }
}
