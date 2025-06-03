using Arbeidstilsynet.Common.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shouldly;
using Xunit;

namespace AT.Common.AspNetCore.Extensions.Test.Unit;

public class StartupExtensionsTests
{
    [Theory]
    [InlineData("PascalCase", "pascal-case")]
    [InlineData("LongAppName", "long-app-name")]
    [InlineData("LongAppNameWithNumbers123", "long-app-name-with-numbers123")]
    [InlineData("camelCase", "camel-case")]
    public void ConvertToOtelServiceName_ValidInput_ShouldReturnConvertedName(
        string input,
        string expectedOutput
    )
    {
        // Act
        var result = input.ConvertToOtelServiceName();

        // Assert
        result.ShouldBe(expectedOutput);
    }
}
